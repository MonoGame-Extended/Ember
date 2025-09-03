# Contributing to MonoGame Extended

Welcome to MonoGame Extended! We're excited that you want to help expand the MonoGame ecosystem with additional functionality and tools.

MonoGame Extended is a community-driven extension library that adds features like sprite batching, input management, collision detection, content pipeline extensions, and much more to MonoGame. As an extension library, we have different priorities and workflows compared to the core MonoGame framework.

## Getting Started

### Understanding Our Mission

MonoGame Extended is a collection of practical utilities designed to help you **prototype quickly and evolve into production** without throwing away your work. We're guided by four core principles:

**Prototype to Production**: Our tools help you manifest early prototypes and evolve them into working games without starting over. We focus on solving present-day pain points rather than anticipating future needs.

**Pragmatic, Not Opinionated**: We're not an engine or framework - we're decoupled utilities that solve specific game development problems. There's no "one right way" to make games, so we provide flexible tools that adapt to your constraints.

**Hardware-Aware**: When beneficial, our code leverages platform-specific capabilities rather than abstracting them away. Different hardware has different strengths, and good game development tools should respect that.

**Expert-Enabling**: We help you become a better game developer by providing well-documented, understandable solutions. Our goal is to save you time and share knowledge, not replace your expertise.

### Development Workflow

We use a **feature branch workflow** with two main branches:

- `main` - Stable releases and hotfixes
- `develop` - Active development and integration

**All contributions should target the `develop` branch.**

## Ways to Contribute

### 🐛 Bug Reports and Fixes

Found a bug? First, check our [existing issues](https://github.com/MonoGame-Extended/MonoGame.Extended/issues) to avoid duplicates. When creating a bug report, our template will guide you through providing:

- Clear description of the problem
- Step-by-step reproduction instructions
- Expected vs actual behavior
- Configuration details (versions, OS, architecture)
- Error messages and stack traces when available

**Pro tip**: Include a minimal code example or link to a reproduction repo when possible. Text is preferred over screenshots for searchability.

### ✨ Feature Requests and Implementation

Feature requests require thoughtful consideration since they affect the entire community. Our feature request template asks you to explain:

- **Purpose**: What problem does this feature solve?
- **Motivation**: Why is this important for game developers?

Before implementing any new feature:

1. **Submit a feature request** using our template
2. **Wait for community discussion** - feature requests need review and approval
3. **Consider the scope** - ensure it aligns with our utility-focused approach
4. **Plan for testing** - new features need comprehensive test coverage

### 📚 Documentation and Examples

Help other developers by:

- Improving API documentation
- Creating usage examples
- Writing tutorials or guides
- Updating the wiki

## Contribution Guidelines

### Code Quality Standards

**Architecture Principles:**

- Follow MonoGame's existing patterns and conventions
- Design for extensibility - other developers should be able to build on your work
- Prefer composition over inheritance where appropriate
- Keep dependencies minimal and well-justified

**Code Style:**

- Follow C# coding conventions and the existing codebase style
- Use meaningful names that clearly express intent
- Write self-documenting code with appropriate comments for complex logic
- Ensure your code works across MonoGame's supported platforms

**Testing Requirements:**

- Include unit tests for new functionality
- Test edge cases and error conditions
- Verify cross-platform compatibility when possible
- Update existing tests if your changes affect them

### Pull Request Process

1. **Fork and branch** from `develop`
2. **Make focused commits** with clear, descriptive messages
3. **Complete the PR checklist** - we won't review incomplete submissions:
   - Verify no overlapping PRs exist
   - Follow contribution guidelines and code of conduct
   - Write a descriptive title
   - Provide appropriate test coverage
4. **Fill out the PR template** with:
   - Clear description of purpose and problem solved
   - Links to related issues (use "closes #123" to auto-close issues)
   - High-level overview of technical changes
5. **Respond to feedback** during the review process

**PR Best Practices:**

- Keep PRs reasonably sized and focused on a single concern
- Include tests that verify your changes work correctly
- Update documentation for any API changes
- Ensure CI passes - our GitHub Actions will verify your build
- Be patient during review - we're all volunteers!

### API Design Considerations

As an extension library, we have special considerations:

**Compatibility:**

- Don't break existing MonoGame Extended APIs without strong justification
- Consider deprecation paths for necessary breaking changes
- Ensure new features integrate well with existing components

**Performance:**

- Profile performance-critical code paths
- Consider memory allocation patterns
- Document any performance implications of new features

**Platform Support:**

- Test on multiple platforms when possible
- Be mindful of platform-specific limitations
- Use conditional compilation when necessary

## Important Restrictions

### Intellectual Property

- **Only submit code you wrote personally** or that you have explicit permission to contribute
- **Never use decompiled code** from any source, including XNA, MonoGame, or other game engines
- **Respect third-party licenses** - if you want to integrate external code, discuss it in an issue first
- **Understand licensing** - your contributions will be distributed under the MIT License

### What We Don't Accept

- Decompiled or reverse-engineered code from any source
- Large PRs that change unrelated parts of the codebase
- Style-only changes without functional improvements
- Features that duplicate core MonoGame functionality
- Breaking changes without prior discussion and approval

## Getting Help

Need assistance or have questions?

- **Discord:** Join our [Discord server](https://discord.gg/xPUEkj9) for real-time discussion
- **Issues:** Use GitHub issues for bug reports and feature requests
- **Discussions:** Start conversations about design decisions or general questions

## Building the Project

### Prerequisites

- .NET SDK (check the version in our global.json)
- MonoGame (compatible version specified in our dependencies)

### Local Development

```bash
git clone https://github.com/MonoGame-Extended/MonoGame.Extended.git
cd MonoGame.Extended
dotnet restore
dotnet build
dotnet test
```

### Content Pipeline Tools

If you're working on content pipeline extensions:

- Test your pipeline extensions with sample content
- Document any new content processor parameters

## Release Process

MonoGame Extended follows semantic versioning:

- **Major versions** for breaking changes
- **Minor versions** for new features
- **Patch versions** for bug fixes

We maintain compatibility with the latest stable MonoGame release and typically support one previous major version.

## Recognition

Contributors are recognized in our release notes and README. Significant contributors may be invited to join the maintainer team.

---

Thank you for helping make MonoGame Extended better for the entire community!

💖 The MonoGame Extended Team
