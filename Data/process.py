"""
CICIDS2017 → Quantum Cyber Analyzer CSV Converter
===================================================
Dataset: https://www.kaggle.com/datasets/chethuhn/network-intrusion-dataset

Steps:
  1. Download and extract the Kaggle dataset
  2. Place this script in the same folder as the CSV files
  3. Run:  python convert.py
  4. Upload the output file to POST /upload

Output columns match your API exactly:
  source_ip, dest_ip, protocol, packet_size, timestamp
"""

import pandas as pd
import os
import sys

# ── Config ────────────────────────────────────────────────────────────────────
# Best file to use — has clear DDoS attack patterns
# Change this to any other file from the dataset if you want
PREFERRED_FILE = "Friday-WorkingHours-Afternoon-DDos.pcap_ISCX.csv"

# Fallback order if preferred file not found
FALLBACK_FILES = [
    "Wednesday-workingHours.pcap_ISCX.csv",
    "Thursday-WorkingHours-Morning-WebAttacks.pcap_ISCX.csv",
    "Tuesday-WorkingHours.pcap_ISCX.csv",
    "Monday-WorkingHours.pcap_ISCX.csv",
]

OUTPUT_FILE = "network_traffic_ready.csv"
ROWS_TO_EXPORT = 1000   # increase to 5000 for more data

# ── Protocol mapping ──────────────────────────────────────────────────────────
PROTOCOL_MAP = {
    0:  "OTHER",
    1:  "ICMP",
    6:  "TCP",
    17: "UDP",
    41: "IPv6",
    58: "ICMPv6",
}

# ── Find which file to use ────────────────────────────────────────────────────
def find_csv_file():
    # Check preferred file first
    if os.path.exists(PREFERRED_FILE):
        return PREFERRED_FILE

    # Check fallbacks
    for f in FALLBACK_FILES:
        if os.path.exists(f):
            print(f"[INFO] Preferred file not found. Using: {f}")
            return f

    # Scan for any CSV in current directory
    csvs = [f for f in os.listdir(".") if f.endswith(".csv")]
    if csvs:
        print(f"[INFO] Found CSV file: {csvs[0]}")
        return csvs[0]

    print("❌ ERROR: No CSV file found in current directory.")
    print("   Make sure you extracted the Kaggle dataset here.")
    sys.exit(1)

# ── Main ──────────────────────────────────────────────────────────────────────
def main():
    input_file = find_csv_file()
    print(f"\n📂 Loading: {input_file}")

    # Load CSV
    df = pd.read_csv(input_file, low_memory=False)

    # Strip whitespace from column names (CICIDS2017 has leading spaces)
    df.columns = df.columns.str.strip()

    print(f"✅ Loaded {len(df):,} rows")
    print(f"📋 Columns found: {df.columns.tolist()}\n")

    # ── Show attack label distribution ────────────────────────────────────────
    if "Label" in df.columns:
        print("🔍 Traffic breakdown:")
        print(df["Label"].value_counts().to_string())
        print()

    # ── Validate required columns exist ───────────────────────────────────────
    required = {
        "Source IP":                       " Source IP",
        "Destination IP":                  " Destination IP",
        "Protocol":                        " Protocol",
        "Total Length of Fwd Packets":     " Total Length of Fwd Packets",
        "Timestamp":                       " Timestamp",
    }

    # Try both stripped and non-stripped versions
    col_map = {}
    for clean, raw in required.items():
        if clean in df.columns:
            col_map[clean] = clean
        elif raw in df.columns:
            col_map[clean] = raw
        else:
            print(f"❌ Missing column: '{clean}' — columns available: {df.columns.tolist()}")
            sys.exit(1)

    # ── Build output dataframe ────────────────────────────────────────────────
    print("⚙️  Converting columns...")

    # Source and destination IPs
    source_ip = df[col_map["Source IP"]].astype(str).str.strip()
    dest_ip   = df[col_map["Destination IP"]].astype(str).str.strip()

    # Protocol — map number to name
    protocol_raw = pd.to_numeric(df[col_map["Protocol"]], errors="coerce").fillna(0).astype(int)
    protocol     = protocol_raw.map(PROTOCOL_MAP).fillna("OTHER")

    # Packet size — use absolute value, drop negatives/zeros
    packet_size = pd.to_numeric(
        df[col_map["Total Length of Fwd Packets"]], errors="coerce"
    ).fillna(0).abs().astype(int)

    # Timestamp — parse and reformat
    timestamp_raw = df[col_map["Timestamp"]].astype(str).str.strip()
    timestamp = pd.to_datetime(timestamp_raw, dayfirst=True, errors="coerce") \
                  .dt.strftime("%Y-%m-%d %H:%M:%S")

    # ── Combine ───────────────────────────────────────────────────────────────
    output = pd.DataFrame({
        "source_ip":   source_ip,
        "dest_ip":     dest_ip,
        "protocol":    protocol,
        "packet_size": packet_size,
        "timestamp":   timestamp,
    })

    # ── Clean up ──────────────────────────────────────────────────────────────
    before = len(output)

    # Drop rows with missing IPs, bad timestamps, zero packet sizes
    output = output.dropna()
    output = output[output["source_ip"].str.match(r"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$")]
    output = output[output["dest_ip"].str.match(r"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$")]
    output = output[output["packet_size"] > 0]
    output = output[output["timestamp"] != "NaT"]

    after = len(output)
    print(f"🧹 Cleaned: {before - after:,} invalid rows removed")

    # ── Sample rows ───────────────────────────────────────────────────────────
    # Take a mix: keep some attack rows + some normal rows for good analysis
    if "Label" in df.columns:
        df_clean = df.loc[output.index]

        attack_rows = output[df_clean["Label"].str.strip() != "BENIGN"]
        normal_rows = output[df_clean["Label"].str.strip() == "BENIGN"]

        # Take 60% attack, 40% normal for visible threat detection
        n_attack = min(len(attack_rows), int(ROWS_TO_EXPORT * 0.6))
        n_normal = min(len(normal_rows), ROWS_TO_EXPORT - n_attack)

        sampled = pd.concat([
            attack_rows.head(n_attack),
            normal_rows.head(n_normal)
        ]).sample(frac=1, random_state=42).reset_index(drop=True)   # shuffle

        print(f"\n📊 Sample composition:")
        print(f"   Attack rows : {n_attack}")
        print(f"   Normal rows : {n_normal}")
        print(f"   Total       : {len(sampled)}")
    else:
        sampled = output.head(ROWS_TO_EXPORT).reset_index(drop=True)

    # ── Save ──────────────────────────────────────────────────────────────────
    sampled.to_csv(OUTPUT_FILE, index=False)

    print(f"\n✅ Saved → {OUTPUT_FILE}")
    print(f"   Rows    : {len(sampled)}")
    print(f"\n📋 Preview:")
    print(sampled.head(5).to_string())

    # print(f"""
# ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
# 🚀 Next Steps:
#    1. Upload to API:
#       POST https://localhost:7011/upload
#       (use file: {OUTPUT_FILE})

#    2. Run analysis:
#       POST https://localhost:7011/analyze

#    3. View threats:
#       GET  https://localhost:7011/threats
# ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
# """)

if __name__ == "__main__":
    main()