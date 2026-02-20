# CAPCOM Master Log

*Append-only. Grep-only. Never read fully.*

## [2026-02-20] System Initialized

Space-Agents installed. HOUSTON standing by.

## [2026-02-20 18:00] Session 1

**Branch:** main | **Git:** uncommitted (new .beads/, .space-agents/, AGENTS.md, modified CLAUDE.md)

### What Happened
Full architecture brainstorm and planning session for BuildScope POC. No code written — this was pure design and task breakdown.

1. **Brainstorm phase:** Explored the Archie Copilot reference codebase via research agent. Made all key architecture decisions through interactive Q&A:
   - Stack: Supabase (pgvector + Edge Functions) + Gemini (embeddings + LLM) — all free tier
   - Dropped Pinecone in favor of Supabase pgvector (simpler, one less service)
   - Revit add-in ported from Archie Copilot pattern (WPF dockable pane)
   - Project context: local JSON, fields are building class, state/territory, type of construction (A/B/C)
   - Per-project chat with ~20 message session memory, session-only
   - NCC chunking: section-based with metadata (volume, part, section, applicable_classes)

2. **Wireframes:** Designed 5 screens — first run welcome, create project form, main chat view (dropdown + chat area), project dropdown expanded, settings panel. Chat answers show inline bold NCC references + grouped references footer.

3. **Spec created:** `.space-agents/mission/staged/tde.1-buildscope-poc/spec.md` — full architecture, wireframes, requirements, constraints, open questions.

4. **Planning phase:** Convened 3-agent council (task-planner, sequencer, implementer). Council produced detailed task breakdown, dependency analysis, and concrete implementation code for all Mac-side tasks (DB schema SQL, Python ingestion script, Edge Function TypeScript).

5. **Beads created:** 1 epic (BuildScope MVP), 1 feature (BuildScope POC), 7 tasks with full dependency chains. Two phases: Mac (tasks 1-3: Supabase + Python) then Windows (tasks 4-7: Revit add-in).

### Decisions Made
- Supabase pgvector over Pinecone — fewer services, free tier handles NCC volume
- Gemini 2.0 Flash for LLM, text-embedding-004 for embeddings — same ecosystem, both free
- Section-based NCC chunking with rich metadata — enables filtering by building class and volume
- Fire rating is Type A/B/C construction (not FRL numbers) — user corrected this during brainstorm
- Code-behind pattern (no MVVM) — matches Archie Copilot, fast for POC
- Merged original 9 tasks into 7 — combined services tasks (5+6) and views tasks (7+8)

### Next Action
Start Task 1: Set up Supabase project and database schema (buildscope-tde.1.1). Can do on Mac right now.

---

## [2026-02-20 13:30] Session 2

**Branch:** main | **Git:** clean

### What Happened
Orchestrated execution of all 3 Mac-side tasks (1.1, 1.2, 1.3). Full Pathfinder/Builder/Inspector cycle on each. Backend RAG pipeline is complete.

1. **Task 1.1 - Supabase setup** (Inspector: 5/5 PASS)
   - Created Supabase project `buildscope` (pibdfbmyhqxckqdeffjg, ap-southeast-2)
   - Enabled pgvector v0.8.0, created `ncc_chunks` table with HNSW + GIN indexes
   - Created `match_ncc_chunks` RPC function with `search_path = 'public', 'extensions'`
   - RLS enabled with anon SELECT policy. Security advisor clean.
   - Set GEMINI_API_KEY as Edge Function secret
   - Created `.env`, `.env.example`, `.gitignore`

2. **Task 1.2 - Python ingestion** (Inspector: 7/7 PASS)
   - Built `ingestion/ingest.py` (349 lines): TOC-based chunking with PyMuPDF, section ID extraction, paragraph-boundary splitting for long sections
   - Ingested NCC 2022 Volume 2 only (user decision to scope down for POC)
   - 363 chunks, 208 unique sections, avg 1016 chars, max 2000, zero oversized
   - Skipped state appendices (Schedules 4-11) and reference schedules
   - Dry-run mode works without API keys

3. **Task 1.3 - Edge Function** (deployed, tests deferred)
   - Deployed `ncc-query` Edge Function (v8, ACTIVE) via Supabase MCP
   - Full RAG pipeline: embed query, pgvector search with class/volume filtering, Gemini 2.0 Flash LLM, reference extraction from bold patterns
   - Input validation (400 for missing fields), CORS, empty results handling
   - Curl tests hit Gemini daily rate limit from ingestion run -- deferred live verification
   - Code saved locally at `supabase/functions/ncc-query/index.ts`

4. **README** - Created project README matching user's GitHub style (archie-copilot pattern)

### Decisions Made
- NCC Volume 2 only for POC (user decision) -- classes 1, 1a, 10a-c
- Switched from deprecated `text-embedding-004` to `gemini-embedding-001` (768 dims) -- Edge Function must match
- Created fresh Supabase project (not reusing inactive `buildspec`)
- Next session: attempt Revit add-in tasks (1.4-1.7) on Mac -- can write code but can't build WPF on macOS

### Gotchas
- `text-embedding-004` is deprecated/removed from Gemini API as of Jan 2026. Use `gemini-embedding-001` everywhere.
- Setting `search_path = ''` on pgvector RPC function breaks the `<=>` operator. Must include `extensions` schema: `set search_path = 'public', 'extensions'`
- Gemini free tier daily quota for `generateContent` exhausts quickly when agents iterate test calls. Builder agent burned through the daily limit during deploy/test cycles.
- Supabase `execute_sql` multi-statement queries roll back entirely on error (no partial commits)

### Next Action
Start Phase 2: Revit add-in tasks (1.4-1.7) on Mac. Write all C# code, defer build/test to Windows. Task 1.4 (scaffold from Archie Copilot) is first.

---

## [2026-02-20 20:30] Session 3

**Branch:** main | **Git:** clean

### What Happened
Orchestrated execution of all 4 Revit add-in tasks (1.4-1.7), plus design upgrade, code review, rename, and Volume 1 ingestion. Massive session.

1. **Task 1.4 - Scaffold Revit add-in** (Inspector: 5/5 PASS)
   - Created `revit-addin/` with BuildSpec.sln, .csproj, .addin, App.cs, ChatPanel.xaml/.cs
   - Ported IExternalApplication, IDockablePaneProvider, ribbon tab from Archie Copilot
   - Stripped IronPython, unique GUIDs, correct BuildSpec namespace

2. **Task 1.5 - Models & services** (Inspector: 6/6 PASS)
   - ChatMessage.cs (INotifyPropertyChanged), ProjectContext.cs ([JsonProperty] for snake_case)
   - Config.cs (env var + config.json fallback), BuildSpecService.cs (HTTP client matching Edge Function contract)
   - ProjectManager.cs (file-based CRUD in AppData), 27 unit tests

3. **Task 1.6 - All views** (Inspector: 8/8 PASS)
   - ChatPanel.xaml/.cs: full chat UI, ChatMessageTemplateSelector, markdown rendering via MarkdownParser.cs, references footer, project dropdown
   - ProjectForm.xaml/.cs: building class/state/type dropdowns, validation
   - SettingsPanel.xaml/.cs: Supabase URL + API key config
   - MainPanel.xaml/.cs: IDockablePaneProvider host with ContentControl navigation
   - 9 MarkdownParser unit tests

4. **Task 1.7 - Wire up chat flow** (Inspector: 7/7 PASS)
   - ChatSessionManager.cs: per-project Dictionary<string, List<ChatMessage>>, 20-message FIFO cap, last-10 API history
   - Wired into ChatPanel: save/restore on project switch, enforce cap after each response
   - 11 unit tests. Total: 47/47 passing

5. **Design upgrade** - Rewrote all 4 XAML views from dark theme to warm light theme inspired by user's reference images at ~/Downloads/inspiration/. Palette: cream background (#F5F1EB), sage green accent (#2B4D3F), white cards, pill buttons (20px radius), bold editorial typography. Updated code-behind hardcoded colors.

6. **Code review** - Full quality review by subagent. Found 2 critical (Config caching bug, ProjectManager path traversal), 5 warnings. Fixed 5 of 9 issues: Config empty env var bug, path sanitization, .sln missing test project, PasswordBox for API key, simplified MainPanel branching.

7. **Rename BuildScope -> BuildSpec** - Full refactor across 24 files, 6 file renames. Zero "BuildScope" references remaining. GitHub repo renamed via `gh repo rename buildspec`.

8. **README rewrite** - Stripped to portfolio-first format matching archie-copilot style. Removed project structure tree, env vars, setup details. Added Class 2 fire resistance example.

9. **Feature closed** - buildscope-tde.1 (BuildSpec POC) closed with all 7 tasks complete.

10. **Volume 1 ingestion** (buildscope-tde.2, in progress)
    - Updated `ingestion/ingest.py` with `--volume` and `--skip` CLI args for multi-volume support
    - Ingested 1,248 / 1,407 Volume 1 chunks before hitting Gemini free tier daily cap (1,000 embed requests)
    - All 1,248 uploaded chunks verified: volume=1, applicable_classes=[2,3,4,5,6,7,8,9], embeddings present

### Decisions Made
- Renamed BuildScope -> BuildSpec (user preference, "spec" maps better to compliance/specifications domain)
- Warm light theme over dark (cream/sage green, inspired by user's reference images -- editorial, architectural feel)
- README targets employers, not contributors -- portfolio-first format
- Class 2 Victoria fire resistance as the README example query
- Volume 1 ingestion to complete NCC coverage (classes 2-9)

### Gotchas
- Gemini free tier embedding quota: 1,000 requests/day per model per project. Burns through fast when ingesting 1,400 chunks (71 batches of 20). Resets at midnight Pacific.
- Config.cs caching bug: empty env var "" assigned to field, then `??=` in LoadFromFile() skips it (not null, just empty). Fix: only cache non-empty values.
- WPF PasswordBox has no Style/Template support like TextBox -- had to inline the styling properties directly on the element.

### In Progress
- **buildscope-tde.2**: 159 chunks remaining for Volume 1 ingestion. Blocked by Gemini daily quota.

### Next Action
**FIRST THING next session:** Finish Volume 1 ingestion (8 API calls, ~2 minutes):
```bash
cd ingestion && source .venv/bin/activate
python ingest.py /Users/fraserbrown/Downloads/ncc2022-volume-one.pdf --volume 1 --skip 1248
```
Then close buildscope-tde.2. After that: build on Windows with Revit 2025 and record demo video.

---

