# IAFahim Intents System

A high-performance Intent-based architecture system for Unity using Data-Oriented Design (DOD) principles with full ECS support.

## Limitations

- Requires Unity 2022.3+ for Entities package compatibility
- Editor tools require Unity Editor (not available in builds)
- Burst compilation requires Burst package 1.8+

## Performance Considerations

- **Layer B (Logic)**: Always `static` and `[MethodImpl(MethodImplOptions.AggressiveInlining)]`
- **Layer A (Data)**: Use `[StructLayout(LayoutKind.Sequential)]` for cache efficiency
- **ECS Path**: Use `IJobEntity` for massive parallelization
- **Avoid**: Virtual methods, interfaces in hot paths, boxing allocations

## Contributing

This package follows strict architectural guidelines. All contributions must:

1. Follow the 4-layer architecture pattern
2. Use side-comment only (no block comments)
3. Implement explicit interfaces for components
4. Return `bool` with `out` parameters
5. Include fail-fast guard clauses

## Getting Help

- **Documentation**: See `Documentation~/com.iafahim.intents.md`
- **Issues**: [GitHub Issues](https://github.com/IAFahim/com.iafahim.intents/issues)
- **Discussions**: [GitHub Discussions](https://github.com/IAFahim/com.iafahim.intents/discussions)

## License

Unity Companion License for Unity-dependent projects. See [LICENSE.md](LICENSE.md) for details.

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for version history.
