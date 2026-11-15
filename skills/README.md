# Agent skills

Skills that teach AI coding agents to use this SDK correctly.

| Skill | Purpose |
|---|---|
| [`google-generativeai-dotnet`](google-generativeai-dotnet/SKILL.md) | Write and upgrade .NET code that uses the Google_GenerativeAI packages. |

## Install

**Claude Code:** copy the skill folder into your personal or project skills directory:

```bash
# Personal (all projects)
git clone --depth 1 https://github.com/gunpal5/Google_GenerativeAI.git /tmp/gga
cp -r /tmp/gga/skills/google-generativeai-dotnet ~/.claude/skills/

# Or for one project
cp -r /tmp/gga/skills/google-generativeai-dotnet <your-project>/.claude/skills/
```

**Other agents:** point the agent at [`llms.txt`](../llms.txt) (documentation index) or
[`llms-full.txt`](../llms-full.txt) (all documentation in one file), or paste `SKILL.md` into its instructions.
