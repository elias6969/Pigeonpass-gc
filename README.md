# PigeonPass – Visitor Registration System
![.NET](https://img.shields.io/badge/.NET-8.0-blue)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15-blue)
![Google Cloud Run](https://img.shields.io/badge/Cloud%20Run-deployed-brightgreen)
![GitHub Pages](https://img.shields.io/badge/GitHub%20Pages-hosted-lightgrey)

## Description

PigeonPass is a cloud-based **visitor registration system**, originally designed for **Azure** but later migrated to **Google Cloud Platform (GCP)** due to subscription and region limitations.
Visitors register through a web form, and the backend API stores their information in a database and logs the request.

---

## System Architecture

### Planned on Azure

* **Static Web Apps** → frontend (HTML + CSS + JS)
* **Azure Functions** → backend API
* **Azure SQL Database** → data storage
* **Application Insights** → logging

### Implemented on Google Cloud

* **GitHub Pages** → frontend hosting
* **Cloud Run (.NET 8 minimal API in Docker)** → backend
* **Cloud SQL (PostgreSQL)** → database
* **Cloud Logging** → logging

---

## Features

* Web form collects:

  * **Name** (required)
  * **Email** (required, validated)
  * **Company** (optional)
  * **Purpose** (e.g., meeting, delivery)

* Form submits data via **POST** request to backend API.

* Backend:

  * Accepts JSON payload.
  * Saves visitor info into database.
  * Logs the request.

### Database Schema

```sql
CREATE TABLE visitors (
    id SERIAL PRIMARY KEY,
    name TEXT NOT NULL,
    email TEXT NOT NULL,
    company TEXT,
    purpose TEXT
);
```

---

## Project Structure

```
Pigeonpass-gc/
│── backend/                # .NET backend (Cloud Run)
│   ├── Program.cs
│   ├── VisitorsController.cs
│   └── ...
│
│── frontend/           # static frontend
│   ├── index.html
│   ├── style.css
│   └── script.js
│
│── Dockerfile          # builds backend container
│── README.md           # this file
```

---

## Setup & Running

### 1. Clone the repository

```bash
git clone git@github.com:elias6969/Pigeonpass-gc.git
cd Pigeonpass-gc
```

### 2. Backend – Run locally

Requires **.NET 8 SDK** and **PostgreSQL** installed locally.

```bash
cd backend
dotnet run
```

The API runs at:
`http://localhost:5173//register`

### 3. Backend – Deploy to Google Cloud Run

Build and push Docker image:

```bash
gcloud builds submit \
  --tag europe-north1-docker.pkg.dev/<project-id>/pigeonpass-repo/pigeonpass-backend
```

Deploy service:

```bash
gcloud run deploy pigeonpass-backend \
  --image=europe-north1-docker.pkg.dev/<project-id>/pigeonpass-repo/pigeonpass-backend \
  --region=europe-north1 \
  --add-cloudsql-instances=pigeonpass-db \
  --set-env-vars=DB_CONNECTION_STRING="Host=/cloudsql/<instance-connection>;Database=visitorsdb;Username=appuser;Password=..."
```

### 4. Frontend – Local test

```bash
cd frontend
python3 -m http.server 5500
```

Open browser at:
`http://localhost:5500`

For production, deploy frontend via **GitHub Pages**

---

## Demo Workflow

1. Open the web form in a browser.
2. Enter **Name**, **Email**, optional **Company** and **Purpose**.
3. Submit -> backend returns confirmation message:
   `Welcome <Name>, your visit has been registered.`
4. Verify data in the database:

```sql
SELECT * FROM visitors;
```

Example output:

```
 id |    name    |        email         | company  | purpose
----+------------+----------------------+----------+----------
  1 | Testy      | test@example.com     | TestCorp | Demo
  2 | Elias Test | elias.test@gmail.com | Nike     | Meeting
  3 | Nidoroi    | nidoroi@gmail.com    | Adidas   | Delivery
```

## License
This project is released under the MIT License
