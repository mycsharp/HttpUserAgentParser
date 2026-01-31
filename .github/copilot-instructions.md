# GitHub Copilot Instructions

## 1. Repository Context & Mission

**Repository:** `HttpUserAgentParser` (mycsharp)

**Primary Goal:**  
Provide a **high-performance, stable, and broadly compatible .NET library** for parsing HTTP User-Agent strings, including integrations for ASP.NET Core and MemoryCache.

**Core Design Principles:**
- API stability over convenience
- Predictable performance characteristics
- Minimal allocations in hot paths
- Full test coverage for all observable behavior

---

## 2. Repository Structure (Authoritative)

Copilot must understand and respect the architectural boundaries:

- `src/HttpUserAgentParser`  
  → Core parsing logic and public APIs

- `src/HttpUserAgentParser.AspNetCore`  
  → ASP.NET Core integration (middleware, extensions)

- `src/HttpUserAgentParser.MemoryCache`  
  → Caching extensions and cache-aware abstractions

- `tests/*`  
  → Unit tests for **all** shipped packages  
  → Tests define expected behavior and are the source of truth

- `perf/*`  
  → Benchmarks for performance-sensitive code paths

---

## 3. Standard .NET CLI Commands

Use these commands consistently:

- Clean:  
  `dotnet clean --nologo`

- Restore:  
  `dotnet restore`

- Build:  
  `dotnet build --nologo`

- Test (all):  
  `dotnet test --nologo`

- Test (single project):  
  `dotnet test <path-to-csproj> --nologo`

---

## 4. Autonomous Execution Rules (Critical)

Copilot is expected to work **independently and end-to-end** without human intervention.

### Mandatory Quality Gates (Never Skip)
- The solution **must compile** after every change
- **All tests must pass**
- New behavior **must include unit tests**
- Public APIs **must not break** existing users unless explicitly intended
- Changes must be **minimal, focused, and intentional**

If any gate fails:
1. Diagnose the root cause
2. Fix the issue
3. Re-run the full validation cycle

---

## 5. Change Strategy & Scope Control

When solving a task, Copilot should:

1. **Analyze existing code first**
   - Prefer extension over modification
   - Reuse established patterns and helpers
2. **Avoid architectural rewrites**
   - No refactors unless explicitly required
3. **Preserve backward compatibility**
   - No breaking changes to public APIs
   - No silent behavioral changes

If multiple solutions are possible:
- Prefer the **simplest**, **most explicit**, and **least invasive** option

---

## 6. Testing Requirements

Every functional change must be fully tested:

- Unit tests are mandatory for:
  - New features
  - Bug fixes
  - Edge cases and regressions
- Prefer existing utilities from:  
  `tests/HttpUserAgentParser.TestHelpers`

Tests define correctness. If behavior is unclear, tests take precedence over assumptions.

---

## 7. Performance Guidelines

- Treat parsing logic as performance-critical
- Avoid unnecessary allocations and LINQ in hot paths
- Prefer spans, pooling, and cached results where appropriate
- Update or add benchmarks in `perf/*` for performance-relevant changes

---

## 8. Output Expectations

Copilot should deliver:
- Compilable, production-ready code
- Complete test coverage for new behavior
- Clear, intentional commits without unrelated changes

**Do not stop early.**  
A task is only complete when **all quality gates pass** and the solution is fully validated.
