using Microsoft.EntityFrameworkCore.Storage;

namespace DbMockInMemory.Tests;

// EF's InMemory provider only reliably shares a named database across
// different DbContext/scope instances when they're all pointed at the same
// InMemoryDatabaseRoot. Without it, store-sharing falls back to EF's own
// service-provider caching, which - across WebApplicationFactory's layered
// DI containers and NUnit's parallel fixtures - proved unreliable: seeded
// (HasData) rows and previously-written rows would randomly appear missing
// from a different scope even with the exact same database name.
public static class InMemoryRoot
{
    public static readonly InMemoryDatabaseRoot Instance = new();
}
