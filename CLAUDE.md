# Claude Code Project Memory

## Git Conventions

### Commit Authorship
- **Author Name**: Gunpal Jain
- **Author Email**: gunpal5@gmail.com
- **Do NOT use "Claude" or "noreply@anthropic.com"** - all commits should be authored by Gunpal Jain

### Branch Naming
- Use descriptive branch names without "claude" prefix
- Preferred formats:
  - `feature/description` - for new features
  - `fix/description` - for bug fixes
  - `refactor/description` - for refactoring
  - `docs/description` - for documentation
  - `mcp-integration` or similar descriptive names

**Important**: Do NOT use "claude/" prefix in branch names unless technically required by the environment.

### Commit Messages
- Use conventional commit format: `type: description`
- Types: `feat`, `fix`, `docs`, `refactor`, `test`, `chore`
- Keep messages clear and professional
- No need to mention "Claude" or AI assistance in commit messages

## Repository Information

### Project
Google_GenerativeAI - Unofficial C# .NET SDK for Google Generative AI (Gemini) and Vertex AI

### Owner
- Name: Gunpal Jain
- GitHub: gunpal5
- Email: gunpal5@gmail.com

### Key Directories
- `/src/GenerativeAI/` - Core SDK
- `/src/GenerativeAI.Tools/` - Tool implementations
- `/src/GenerativeAI.Microsoft/` - Microsoft.Extensions.AI integration
- `/samples/` - Example projects
- `/tests/` - Test projects

### Current Work
MCP (Model Context Protocol) integration with support for all transport protocols (stdio, HTTP/SSE).

### Code Style
- C# with latest language features
- Nullable reference types enabled
- Comprehensive XML documentation
- Follow existing patterns in the codebase

## Notes
- When creating PRs, ensure clear descriptions and link related issues
- Run tests before committing (when possible)
- Follow semantic versioning for releases
