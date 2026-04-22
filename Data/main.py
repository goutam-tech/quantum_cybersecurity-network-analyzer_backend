"""
NSL-KDD → Quantum Cyber Analyzer CSV Converter
================================================
Dataset: https://www.kaggle.com/datasets/sampadab17/network-intrusion-detection

Files in this dataset:
  - Train_data.csv   ← use this one
  - Test_data.csv

The NSL-KDD dataset has NO real IP addresses.
This script generates realistic IPs based on attack type
so your Quantum Walk can detect attack clusters properly.

Run:
    pip install pandas
    python convert_nslkdd.py

Output: network_traffic_ready.csv  (upload this to POST /upload)
"""

import pandas as pd
import random
import os
import sys
from datetime import datetime, timedelta

# ── Config ────────────────────────────────────────────────────────────────────
INPUT_FILE   = "Data\\input\\Train_data.csv"     # file from Kaggle
OUTPUT_FILE  = "Data\\output\\network_traffic_ready.csv"
ROWS_TO_SAVE = 1000                 # increase up to 5000 if you want more data

# ── IP pools (realistic subnets) ──────────────────────────────────────────────
# Attackers — small pool so same IPs hit many targets (Quantum Walk detects this)
ATTACKER_IPS = [f"192.168.99.{i}" for i in range(1, 16)]

# Normal users — larger pool, varied traffic
NORMAL_IPS   = [f"192.168.1.{i}"  for i in range(1, 60)]

# Servers — targets that receive traffic
SERVER_IPS   = [f"10.0.0.{i}"     for i in range(1, 30)]

# ── Attack type → IP behaviour mapping ───────────────────────────────────────
# DoS/DDoS → same attacker IPs hit many servers (port scan / flood pattern)
# Probe     → one attacker scans all servers
# R2L/U2R  → attacker targets one specific server repeatedly
# Normal    → random user to random server

ATTACK_GROUPS = {
    "dos":    ["back", "land", "neptune", "pod", "smurf", "teardrop",
               "apache2", "udpstorm", "processtable", "worm"],
    "probe":  ["ipsweep", "nmap", "portsweep", "satan", "mscan", "saint"],
    "r2l":    ["ftp_write", "guess_passwd", "imap", "multihop", "phf",
               "spy", "warezclient", "warezmaster", "sendmail", "named",
               "snmpgetattack", "snmpguess", "xlock", "xsnoop", "httptunnel"],
    "u2r":    ["buffer_overflow", "loadmodule", "perl", "rootkit",
               "ps", "sqlattack", "xterm"],
}

def get_attack_group(label: str) -> str:
    label = label.strip().lower()
    if label == "normal":
        return "normal"
    for group, attacks in ATTACK_GROUPS.items():
        if label in attacks:
            return group
    return "dos"   # default unknown attacks to dos group

def assign_ips(label: str):
    """Assign source and destination IPs based on attack type."""
    group = get_attack_group(label)

    if group == "normal":
        # Normal: random user → random server
        return random.choice(NORMAL_IPS), random.choice(SERVER_IPS)

    elif group == "dos":
        # DoS: few attackers → many servers (triggers Quantum Walk anomaly)
        attacker = random.choice(ATTACKER_IPS[:5])   # only 5 attacker IPs
        return attacker, random.choice(SERVER_IPS)

    elif group == "probe":
        # Probe: one scanner → many different servers (port scan pattern)
        scanner = random.choice(ATTACKER_IPS[5:8])   # 3 scanner IPs
        return scanner, random.choice(SERVER_IPS)

    elif group in ("r2l", "u2r"):
        # R2L/U2R: attacker targets ONE specific server repeatedly
        attacker = random.choice(ATTACKER_IPS[8:12])
        target   = SERVER_IPS[0]   # always hits the same server
        return attacker, target

    return random.choice(ATTACKER_IPS), random.choice(SERVER_IPS)

# ── Protocol mapping ──────────────────────────────────────────────────────────
PROTOCOL_MAP = {
    "tcp":  "TCP",
    "udp":  "UDP",
    "icmp": "ICMP",
}

# ── Generate timestamps ───────────────────────────────────────────────────────
def generate_timestamps(df: pd.DataFrame) -> list:
    """
    Generate realistic timestamps.
    Attack bursts happen close together (seconds apart).
    Normal traffic is spread over hours.
    """
    base_time = datetime(2024, 1, 15, 8, 0, 0)
    timestamps = []

    for i, row in df.iterrows():
        label = str(row.get("class", "normal")).strip().lower()
        group = get_attack_group(label)

        if group == "normal":
            # Normal traffic spread over 8 hours
            offset = timedelta(seconds=random.randint(0, 28800))
        elif group == "dos":
            # DoS: rapid fire — milliseconds apart in bursts
            offset = timedelta(
                seconds=random.randint(0, 300),
                milliseconds=random.randint(0, 999)
            )
        elif group == "probe":
            # Probe: seconds apart (scanning)
            offset = timedelta(seconds=random.randint(0, 600))
        else:
            # R2L/U2R: minutes apart (slow, targeted)
            offset = timedelta(minutes=random.randint(0, 120))

        timestamps.append((base_time + offset).strftime("%Y-%m-%d %H:%M:%S"))

    return timestamps

# ── Main ──────────────────────────────────────────────────────────────────────
def main():
    # Check file exists
    if not os.path.exists(INPUT_FILE):
        print(f"❌ File not found: {INPUT_FILE}")
        print(f"   Make sure '{INPUT_FILE}' is in this folder.")
        print(f"   Files here: {os.listdir('.')}")
        sys.exit(1)

    print(f"\n📂 Loading: {INPUT_FILE}")
    df = pd.read_csv(INPUT_FILE)
    df.columns = df.columns.str.strip()

    print(f"✅ Loaded {len(df):,} rows")
    print(f"📋 Columns: {df.columns.tolist()}\n")

    # ── Show class distribution ───────────────────────────────────────────────
    label_col = "class" if "class" in df.columns else "attack_type"
    if label_col in df.columns:
        print("🔍 Attack type breakdown:")
        print(df[label_col].value_counts().head(15).to_string())
        print()
    else:
        print("⚠️  No label column found — will treat all as normal")
        label_col = None

    # ── Validate packet size column ───────────────────────────────────────────
    # NSL-KDD uses src_bytes as packet size
    if "src_bytes" not in df.columns:
        print(f"❌ 'src_bytes' column not found. Columns: {df.columns.tolist()}")
        sys.exit(1)

    # ── Sample: mix of attacks + normal ──────────────────────────────────────
    if label_col:
        attack_mask = df[label_col].str.strip().str.lower() != "normal"
        attack_df   = df[attack_mask]
        normal_df   = df[~attack_mask]

        n_attack = min(len(attack_df), int(ROWS_TO_SAVE * 0.6))
        n_normal = min(len(normal_df), ROWS_TO_SAVE - n_attack)

        sampled = pd.concat([
            attack_df.sample(n=n_attack, random_state=42),
            normal_df.sample(n=n_normal, random_state=42)
        ]).sample(frac=1, random_state=42).reset_index(drop=True)

        print(f"📊 Sample composition:")
        print(f"   Attack rows : {n_attack}")
        print(f"   Normal rows : {n_normal}")
        print(f"   Total       : {len(sampled)}\n")
    else:
        sampled = df.sample(n=min(len(df), ROWS_TO_SAVE), random_state=42).reset_index(drop=True)

    # ── Assign IPs ────────────────────────────────────────────────────────────
    print("⚙️  Generating IPs based on attack patterns...")
    labels = sampled[label_col].tolist() if label_col else ["normal"] * len(sampled)

    src_ips, dst_ips = zip(*[assign_ips(label) for label in labels])

    # ── Build output ──────────────────────────────────────────────────────────
    print("⚙️  Building output CSV...")

    packet_sizes = pd.to_numeric(sampled["src_bytes"], errors="coerce").fillna(64).abs().astype(int)
    packet_sizes = packet_sizes.clip(lower=64, upper=65535)   # realistic range

    protocols = sampled["protocol_type"].str.strip().str.lower().map(PROTOCOL_MAP).fillna("OTHER")

    timestamps = generate_timestamps(sampled)

    output = pd.DataFrame({
        "source_ip":   list(src_ips),
        "dest_ip":     list(dst_ips),
        "protocol":    protocols.values,
        "packet_size": packet_sizes.values,
        "timestamp":   timestamps,
    })

    # ── Save ──────────────────────────────────────────────────────────────────
    output.to_csv(OUTPUT_FILE, index=False)

    print(f"\n✅ Saved → {OUTPUT_FILE}")
    print(f"   Rows    : {len(output)}")
    print(f"\n📋 Preview (first 5 rows):")
    print(output.head(5).to_string())

    print(f"\n🌐 Protocol distribution:")
    print(output["protocol"].value_counts().to_string())

    print(f"\n🔴 Top attacker IPs (should show Attack in results):")
    print(output["source_ip"].value_counts().head(5).to_string())

#     print(f"""
# ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
# 🚀 Next Steps:

# 1. Upload CSV to your API:
#    curl -X POST https://localhost:7011/upload \\
#      -F "file=@{OUTPUT_FILE}"

# 2. Run analysis:
#    curl -X POST https://localhost:7011/analyze

# 3. View threats:
#    curl https://localhost:7011/threats

# 4. Expected results:
#    - 192.168.99.x IPs → Attack (DoS attackers)
#    - 192.168.99.x IPs → Suspicious (Probe/R2L)
#    - 192.168.1.x IPs  → Normal (regular users)
# ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
# """)

if __name__ == "__main__":
    random.seed(42)
    main()