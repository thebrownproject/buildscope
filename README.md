# BuildScope

**NCC compliance, without leaving Revit.**

A dockable side panel for Autodesk Revit 2025 that answers National Construction Code questions using RAG. Define your project context (building class, state, construction type) and ask questions in plain English. BuildScope retrieves relevant NCC sections and returns answers with specific section references, all inline in Revit.

---

## How it works

You create a project with your building's classification (e.g. Class 1, Victoria, Type C construction). When you ask a question like *"What insulation do I need for external walls?"*, the system:

1. Embeds your question via Gemini
2. Searches 363 NCC Volume 2 sections using pgvector similarity with metadata filtering
3. Retrieves the most relevant sections for your building class
4. Sends them to Gemini with your project context
5. Returns a concise answer with inline **section references** and a references footer

The entire pipeline runs on free tiers (Supabase + Gemini). No paid services required.

---

## Tech Stack

**Platform:** WPF · .NET 8 · Revit 2025 API <br>
**Backend:** Supabase Edge Functions (TypeScript/Deno) <br>
**Database:** Supabase PostgreSQL · pgvector <br>
**AI:** Gemini 2.0 Flash (LLM) · gemini-embedding-001 (768-dim embeddings) <br>
**Ingestion:** Python · PyMuPDF · google-genai

---

## Architecture

```
Revit Add-in (C#)          Supabase Edge Function         Supabase Postgres
┌──────────────┐    POST   ┌──────────────────────┐       ┌──────────────┐
│ WPF Side     │ ───────>  │ 1. Embed query       │ ───>  │ pgvector     │
│ Panel        │           │ 2. Vector search     │ <───  │ ncc_chunks   │
│              │ <───────  │ 3. Gemini LLM answer │       │ 363 sections │
│ Project      │   JSON    │ 4. Extract refs      │       └──────────────┘
│ Context      │           └──────────────────────┘
└──────────────┘
```

The Revit add-in sends questions with project context to a Supabase Edge Function. The Edge Function embeds the query, runs a similarity search filtered by building class and volume, sends the top chunks to Gemini with a system prompt, and returns an answer with NCC section references.

NCC content is pre-processed by a Python ingestion script that parses the PDF using TOC-based chunking, generates embeddings, and uploads to Supabase. Each chunk carries metadata (volume, part, section ID, applicable building classes) for filtered retrieval.

---

## Project Structure

```
buildscope/
├── supabase/
│   └── functions/
│       └── ncc-query/
│           └── index.ts          # Edge Function: RAG query pipeline
├── ingestion/
│   ├── ingest.py                 # NCC PDF chunking + embedding + upload
│   └── requirements.txt
├── .env.example                  # Required environment variables
└── CLAUDE.md
```

*Revit add-in files (C#/.NET) are developed on Windows and will be added when that phase begins.*

---

## Setup

### Backend (Mac or Linux)

**1. Supabase**

The project uses a Supabase database with pgvector. The schema includes the `ncc_chunks` table, HNSW/GIN indexes, and a `match_ncc_chunks` RPC function for similarity search. Migrations are applied via the Supabase dashboard or MCP tools.

**2. NCC Ingestion**

```bash
cd ingestion
python -m venv .venv
source .venv/bin/activate
pip install -r requirements.txt

# Dry run (no API calls)
python ingest.py /path/to/ncc2022-volume-two.pdf --dry-run

# Full ingestion
python ingest.py /path/to/ncc2022-volume-two.pdf
```

**3. Environment Variables**

```env
SUPABASE_URL=https://your-project.supabase.co
SUPABASE_ANON_KEY=your-anon-key
SUPABASE_SERVICE_ROLE_KEY=your-service-role-key
GEMINI_API_KEY=your-gemini-api-key
```

### Revit Add-in (Windows)

*Coming soon. Requires Revit 2025 and .NET 8 SDK.*

---

## Current Status

**Backend complete:**
- [x] Supabase project with pgvector, indexes, and RPC function
- [x] NCC Volume 2 ingested (363 chunks, 208 sections)
- [x] Edge Function deployed (query embedding, vector search, Gemini LLM, reference extraction)

**Next up (Windows):**
- [ ] Scaffold Revit add-in from Archie Copilot base
- [ ] Build models and services layer
- [ ] Build WPF views (chat panel, project form, settings)
- [ ] Wire up end-to-end chat flow

---

## Why this project

This demonstrates building a full AI-powered tool for a domain-specific professional application:

- RAG pipeline from scratch (PDF parsing, chunking strategy, embedding, vector search, LLM generation)
- Integrating AI into Autodesk Revit with WPF dockable panels and threading constraints
- Working with regulatory/compliance content (structured metadata, section-based retrieval, citation accuracy)
- End-to-end system design across multiple platforms (Supabase, Gemini, Revit/.NET, Python)
- Shipping on entirely free tiers with no paid infrastructure
