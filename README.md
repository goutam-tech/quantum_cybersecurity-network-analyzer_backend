#  Quantum Cyber Analyzer

A network intrusion detection system that applies **quantum-inspired algorithms** — Quantum Walk and Quantum Fourier Transform (QFT) — to detect anomalous and malicious traffic patterns in network logs.

---

## Problem Statement

Traditional network security tools rely on signature-based detection or simple statistical thresholds, which struggle against novel attacks, slow-burn intrusions, and highly periodic bot traffic. 

**Quantum Cyber Analyzer** addresses this by modeling network activity as a graph and applying quantum-inspired probabilistic techniques to surface anomalies that conventional methods miss:

- **Quantum Walk** identifies nodes (IPs) whose traffic connectivity deviates significantly from the expected probability distribution across the graph.
- **Quantum Fourier Transform (QFT)** detects periodic traffic signatures — a hallmark of automated attacks, botnets, and beaconing malware.

Together, these two scores are fused into a **threat confidence score**, classifying each IP as `Normal`, `Suspicious`, or `Attack`.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Backend Framework | ASP.NET Core 8 (C#) |
| Database | PostgreSQL (via EF Core + Npgsql) |
| ORM | Entity Framework Core |
| Authentication | JWT Bearer Tokens |
| Password Hashing | BCrypt.Net |
| CSV Parsing | Custom `CsvParserHelper` |
| Quantum Algorithms | Custom C# implementations (Quantum Walk, DFT) |
| API Docs | Swagger / OpenAPI |
| Config | `.env` file via DotNetEnv |

---

## How It Works

### 1. Upload Phase (`POST /upload`)
- User uploads a `.csv` file containing network logs.
- CSV is parsed and stored as `NetworkLog` records.
- A graph is built: each unique IP becomes a **Node**, each src→dst connection becomes an **Edge** with a weight (connection count).

### 2. Analysis Phase (`POST /analyze`)

#### Quantum Walk
- The graph adjacency matrix is used to simulate a quantum walk over all IP nodes.
- Each node starts with equal amplitude; over 5 steps, amplitude propagates to neighbors proportionally by edge weight.
- After normalization, probability scores are derived. Nodes deviating significantly from the mean (by z-score) receive a high **anomaly score**.

#### Quantum Fourier Transform (QFT)
- Traffic for each IP is bucketed into a 16-slot time series based on packet sizes.
- A Discrete Fourier Transform is computed over this signal.
- The **dominant frequency** and **periodicity score** (ratio of peak magnitude to total) identify IPs with regular, automated traffic patterns.

#### Threat Scoring
- Both scores are **normalized** across all nodes (max normalization).
- A weighted combination is computed:
  ```
  threatScore = (0.6 × quantumWalkScore) + (0.4 × periodicityScore)
  ```
- Classification:
  - `threatScore ≥ 0.65` → **Attack**
  - `threatScore ≥ 0.35` → **Suspicious**
  - Below → **Normal**

---

## File Structure

```
network_project/
│
├── Controllers/
│   ├── AnalysisControllers.cs   # /analyze, /results, /threats endpoints
│   ├── AuthController.cs        # /auth/signup, login, revoke, me
│   ├── LogsController.cs        # /logs - paginated log viewer
│   └── UploadController.cs      # /upload - CSV ingestion
│
├── Data/
│   └── AppDbContext.cs          # EF Core DbContext with all entity configs
│
├── Dto/
│   ├── AuthDtos.cs              # SignupDto, LoginDto, RevokeDto, AuthResponseDto
│   └── Dtos.cs                  # NetworkLogDto, NodeDto, DetectionResultDto, etc.
│
├── Helper/
│   ├── CsvParserHelper.cs       # CSV parsing and validation
│   ├── GraphBuilderHelper.cs    # Builds Node/Edge graph from logs
│   ├── QuantumWalkHelper.cs     # Quantum walk simulation
│   ├── QftAnalysisHelper.cs     # DFT-based periodicity analysis
│   ├── ThreatScoringHelper.cs   # Fuses QW + QFT scores → threat level
│   ├── JWTHelper.cs             # JWT generation and validation
│   └── PasswordHelper.cs        # BCrypt hashing and verification
│
├── Interfaces/
│   ├── IRepositories.cs         # INetworkLogRepository,INodeRepository, etc.
│   └── IAuthRepositories.cs     # IUserRepository, ITokenRepository
│
├── Middleware/
│   └── GlobalExceptionHandler.cs # Catches unhandled exceptions globally
│
├── Models/
│   ├── NetworkLog.cs            # Raw CSV log record
│   ├── Node.cs                  # IP node with anomaly score
│   ├── Edge.cs                  # Directed weighted connection
│   ├── QuantumWalkResult.cs     # Per-node quantum walk output
│   ├── QftResult.cs             # Per-node DFT output
│   ├── DetectionResult.cs       # Final threat classification
│   ├── User.cs                  # Auth user
│   └── UserToken.cs             # JWT token record
│
├── Repository/
│   ├── BaseRepository.cs        # Generic EF Core CRUD base
│   ├── Repositories.cs          # Node, Edge, Log, QW, QFT, Detection repos
│   └── AuthRepositories.cs      # UserRepository, TokenRepository
│
├── Program.cs                   # App bootstrap, DI, middleware, EF migrations
└── .env                         # Environment variables (not committed)
```

---

## How to Run

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/) running locally or via Docker

### 1. Clone the repository

```bash
git clone https://github.com/your-username/quantum-cyber-analyzer.git
cd quantum-cyber-analyzer
```

### 2. Set up environment variables

Create a `.env` file in the project root:

```env
DB_CONNECTION=Host=localhost;Port=5432;Database=quantum_cyber;Username=postgres;Password=yourpassword
JWT_SECRET_KEY=your_super_secret_key_at_least_32_chars_long
```

### 3. Install dependencies

```bash
dotnet restore
```

### 4. Apply database migrations

```bash
dotnet ef database update
```

> If you haven't created migrations yet:
> ```bash
> dotnet ef migrations add InitialCreate
> dotnet ef database update
> ```

### 5. Run the API

```bash
dotnet run
```

The API will be available at `https://localhost:5001` (or `http://localhost:5000`).

Swagger UI: `http://localhost:5000/swagger`

---

## 📡 API Endpoints

### Auth
| Method | Endpoint | Description |
|---|---|---|
| POST | `/auth/signup` | Register a new user |
| POST | `/auth/login` | Login and receive JWT |
| POST | `/auth/revoke` | Revoke a JWT token |
| GET | `/auth/me` | Get current user info (🔒 JWT required) |

### Upload
| Method | Endpoint | Description |
|---|---|---|
| POST | `/upload` | Upload a CSV file of network logs |

**Expected CSV format:**
```csv
sourceip,destip,protocol,packetsize,timestamp
192.168.1.1,10.0.0.1,TCP,1500,01-06-2024 14:30
```

### Analysis
| Method | Endpoint | Description |
|---|---|---|
| POST | `/analyze` | Run quantum analysis on uploaded data |
| GET | `/results` | Get latest detection results |
| GET | `/results/quantum-walk` | Get top quantum walk anomalies |
| GET | `/results/qft` | Get QFT periodicity results |

### Threats
| Method | Endpoint | Description |
|---|---|---|
| GET | `/threats` | Get threat summary (Attack / Suspicious / Normal) |
| GET | `/threats/{level}` | Get IPs by threat level |

### Logs
| Method | Endpoint | Description |
|---|---|---|
| GET | `/logs` | Get paginated raw network logs |
| GET | `/logs/{id}` | Get a specific log entry |

---

## Example Workflow

```bash
# 1. Sign up
POST /auth/signup
{ "email": "admin@example.com", "name": "Admin", "password": "secret123" }

# 2. Upload CSV
POST /upload
Content-Type: multipart/form-data
file=@network_traffic.csv

# 3. Run analysis
POST /analyze

# 4. View threats
GET /threats
```

---

## Security Notes

- Passwords are hashed with **BCrypt** (work factor 12).
- JWTs are signed with **HMAC-SHA256** and validated on every protected request.
- Tokens can be explicitly revoked via `/auth/revoke`.
- All unhandled exceptions are caught by `GlobalExceptionHandler` — stack traces are never exposed to clients in production.

---

## License

MIT License. See [LICENSE](LICENSE.txt) for details.