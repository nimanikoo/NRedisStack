using NRedisStack.Core;
using NRedisStack.Core.DataTypes;
using StackExchange.Redis;

namespace NRedisStack;

public static class CoreCommands
{
    /// <summary>
    /// Sets information specific to the client or connection.
    /// </summary>
    /// <param name="attr">which attribute to set</param>
    /// <param name="value">the attribute value</param>
    /// <returns><see langword="true"/> if the attribute name was successfully set, Error otherwise.</returns>
    /// <remarks><seealso href="https://redis.io/commands/client-setinfo/"/></remarks>
    public static bool ClientSetInfo(this IDatabase db, SetInfoAttr attr, string value)
    {
        var compareVersions = db.Multiplexer.GetServer(db.Multiplexer.GetEndPoints()[0]).Version.CompareTo(new Version(7, 1, 242));
        if (compareVersions < 0) // the server does not support the CLIENT SETNAME command
            return false;
        return db.Execute(CoreCommandBuilder.ClientSetInfo(attr, value)).OKtoBoolean();
    }

    /// <summary>
    /// The BZMPOP command.
    /// <p/>
    /// Removes and returns up to <paramref name="count"/> entries from the first non-empty sorted set in
    /// <paramref name="keys"/>. If none of the sets contain elements, the call blocks on the server until elements
    /// become available, or the given <paramref name="timeout"/> expires. A <paramref name="timeout"/> of <c>0</c>
    /// means to wait indefinitely server-side. Returns <c>null</c> if the server timeout expires.
    /// <p/>
    /// When using this, pay attention to the timeout configured in the client, on the
    /// <see cref="ConnectionMultiplexer"/>, which by default can be too small:
    /// <code>
    /// ConfigurationOptions configurationOptions = new ConfigurationOptions();
    /// configurationOptions.SyncTimeout = 120000; // set a meaningful value here
    /// configurationOptions.EndPoints.Add("localhost");
    /// ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(configurationOptions);
    /// </code>
    /// If the connection multiplexer timeout expires in the client, a <c>StackExchange.Redis.RedisTimeoutException</c>
    /// is thrown.
    /// <p/>
    /// This is an extension method added to the <see cref="IDatabase"/> class, for convenience.
    /// </summary>
    /// <param name="db">The <see cref="IDatabase"/> class where this extension method is applied.</param>
    /// <param name="timeout">Server-side timeout for the wait. A value of <c>0</c> means to wait indefinitely.</param>
    /// <param name="keys">The keys to check.</param>
    /// <param name="minMaxModifier">Specify from which end of the sorted set to pop values. If set to <c>MinMaxModifier.Min</c>
    /// then the minimum elements will be popped, otherwise the maximum values.</param>
    /// <param name="count">The maximum number of records to pop out. If set to <c>null</c> then the server default
    /// will be used.</param>
    /// <returns>A collection of sorted set entries paired with their scores, together with the key they were popped
    /// from, or <c>null</c> if the server timeout expires.</returns>
    /// <remarks><seealso href="https://redis.io/commands/bzmpop"/></remarks>
    public static Tuple<RedisKey, List<RedisValueWithScore>>? BzmPop(this IDatabase db, double timeout, RedisKey[] keys, MinMaxModifier minMaxModifier, long? count = null)
    {
        var command = CoreCommandBuilder.BzmPop(timeout, keys, minMaxModifier, count);
        return db.Execute(command).ToSortedSetPopResults();
    }

    /// <summary>
    /// Syntactic sugar for
    /// <see cref="BzmPop(StackExchange.Redis.IDatabase,double,StackExchange.Redis.RedisKey[],NRedisStack.Core.DataTypes.MinMaxModifier,System.Nullable{long})"/>,
    /// where only one key is used.
    /// </summary>
    /// <param name="db">The <see cref="IDatabase"/> class where this extension method is applied.</param>
    /// <param name="timeout">Server-side timeout for the wait. A value of <c>0</c> means to wait indefinitely.</param>
    /// <param name="key">The key to check.</param>
    /// <param name="minMaxModifier">Specify from which end of the sorted set to pop values. If set to <c>MinMaxModifier.Min</c>
    /// then the minimum elements will be popped, otherwise the maximum values.</param>
    /// <param name="count">The maximum number of records to pop out. If set to <c>null</c> then the server default
    /// will be used.</param>
    /// <returns>A collection of sorted set entries paired with their scores, together with the key they were popped
    /// from, or <c>null</c> if the server timeout expires.</returns>
    /// <remarks><seealso href="https://redis.io/commands/bzmpop"/></remarks>
    public static Tuple<RedisKey, List<RedisValueWithScore>>? BzmPop(this IDatabase db, double timeout, RedisKey key, MinMaxModifier minMaxModifier, long? count = null)
    {
        return BzmPop(db, timeout, new[] { key }, minMaxModifier, count);
    }

    /// <summary>
    /// The BZPOPMIN command.
    /// <p/>
    /// Removes and returns the entry with the smallest score from the first non-empty sorted set in
    /// <paramref name="keys"/>. If none of the sets contain elements, the call blocks on the server until elements
    /// become available, or the given <paramref name="timeout"/> expires. A <paramref name="timeout"/> of <c>0</c>
    /// means to wait indefinitely server-side. Returns <c>null</c> if the server timeout expires.
    /// <p/>
    /// When using this, pay attention to the timeout configured in the client, on the
    /// <see cref="ConnectionMultiplexer"/>, which by default can be too small:
    /// <code>
    /// ConfigurationOptions configurationOptions = new ConfigurationOptions();
    /// configurationOptions.SyncTimeout = 120000; // set a meaningful value here
    /// configurationOptions.EndPoints.Add("localhost");
    /// ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(configurationOptions);
    /// </code>
    /// If the connection multiplexer timeout expires in the client, a <c>StackExchange.Redis.RedisTimeoutException</c>
    /// is thrown.
    /// <p/>
    /// This is an extension method added to the <see cref="IDatabase"/> class, for convenience.
    /// </summary>
    /// <param name="db">The <see cref="IDatabase"/> class where this extension method is applied.</param>
    /// <param name="keys">The keys to check.</param>
    /// <param name="timeout">Server-side timeout for the wait. A value of <c>0</c> means to wait indefinitely.</param>
    /// <returns>A sorted set entry paired with its score, together with the key it was popped from, or <c>null</c>
    /// if the server timeout expires.</returns>
    /// <remarks><seealso href="https://redis.io/commands/bzpopmin"/></remarks>
    public static Tuple<RedisKey, RedisValueWithScore>? BzPopMin(this IDatabase db, RedisKey[] keys, double timeout)
    {
        var command = CoreCommandBuilder.BzPopMin(keys, timeout);
        return db.Execute(command).ToSortedSetPopResult();
    }

    /// <summary>
    /// Syntactic sugar for <see cref="BzPopMin(StackExchange.Redis.IDatabase,StackExchange.Redis.RedisKey[],double)"/>,
    /// where only one key is used.
    /// </summary>
    /// <param name="db">The <see cref="IDatabase"/> class where this extension method is applied.</param>
    /// <param name="key">The key to check.</param>
    /// <param name="timeout">Server-side timeout for the wait. A value of <c>0</c> means to wait indefinitely.</param>
    /// <returns>A sorted set entry paired with its score, together with the key it was popped from, or <c>null</c>
    /// if the server timeout expires.</returns>
    /// <remarks><seealso href="https://redis.io/commands/bzpopmin"/></remarks>
    public static Tuple<RedisKey, RedisValueWithScore>? BzPopMin(this IDatabase db, RedisKey key, double timeout)
    {
        return BzPopMin(db, new[] { key }, timeout);
    }


    /// <summary>
    /// The BZPOPMAX command.
    /// <p/>
    /// Removes and returns the entry with the highest score from the first non-empty sorted set in
    /// <paramref name="keys"/>. If none of the sets contain elements, the call blocks on the server until elements
    /// become available, or the given <paramref name="timeout"/> expires. A <paramref name="timeout"/> of <c>0</c>
    /// means to wait indefinitely server-side. Returns <c>null</c> if the server timeout expires.
    /// <p/>
    /// When using this, pay attention to the timeout configured in the client, on the
    /// <see cref="ConnectionMultiplexer"/>, which by default can be too small:
    /// <code>
    /// ConfigurationOptions configurationOptions = new ConfigurationOptions();
    /// configurationOptions.SyncTimeout = 120000; // set a meaningful value here
    /// configurationOptions.EndPoints.Add("localhost");
    /// ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(configurationOptions);
    /// </code>
    /// If the connection multiplexer timeout expires in the client, a <c>StackExchange.Redis.RedisTimeoutException</c>
    /// is thrown.
    /// <p/>
    /// This is an extension method added to the <see cref="IDatabase"/> class, for convenience.
    /// </summary>
    /// <param name="db">The <see cref="IDatabase"/> class where this extension method is applied.</param>
    /// <param name="keys">The keys to check.</param>
    /// <param name="timeout">Server-side timeout for the wait. A value of <c>0</c> means to wait indefinitely.</param>
    /// <returns>A sorted set entry paired with its score, together with the key it was popped from, or <c>null</c>
    /// if the server timeout expires.</returns>
    /// <remarks><seealso href="https://redis.io/commands/bzpopmax"/></remarks>
    public static Tuple<RedisKey, RedisValueWithScore>? BzPopMax(this IDatabase db, RedisKey[] keys, double timeout)
    {
        var command = CoreCommandBuilder.BzPopMax(keys, timeout);
        return db.Execute(command).ToSortedSetPopResult();
    }

    /// <summary>
    /// Syntactic sugar for <see cref="BzPopMax(StackExchange.Redis.IDatabase,StackExchange.Redis.RedisKey[],double)"/>,
    /// where only one key is used.
    /// </summary>
    /// <param name="db">The <see cref="IDatabase"/> class where this extension method is applied.</param>
    /// <param name="key">The key to check.</param>
    /// <param name="timeout">Server-side timeout for the wait. A value of <c>0</c> means to wait indefinitely.</param>
    /// <returns>A sorted set entry paired with its score, together with the key it was popped from, or <c>null</c>
    /// if the server timeout expires.</returns>
    /// <remarks><seealso href="https://redis.io/commands/bzpopmax"/></remarks>
    public static Tuple<RedisKey, RedisValueWithScore>? BzPopMax(this IDatabase db, RedisKey key, double timeout)
    {
        return BzPopMax(db, new[] { key }, timeout);
    }
}

/// <summary>
/// The BZMPOP command.
/// <p/>
/// Removes and returns up to <paramref name="count"/> entries from the first non-empty sorted set in
/// <paramref name="keys"/>. If none of the sets contain elements, the call blocks on the server until elements
/// become available or the given <paramref name="timeout"/> passes. A <paramref name="timeout"/> of <c>0</c>
/// means to wait indefinitely server-side. Returns <c>null</c> if the server timeout expires.
/// <p/>
/// When using this, pay attention to the timeout configured on the <see cref="ConnectionMultiplexer"/>, which
/// by default can be too small, in which case you want to increase it:
/// <code>
/// ConfigurationOptions configurationOptions = new ConfigurationOptions();
/// configurationOptions.SyncTimeout = 120000; // set a meaningful value here
/// configurationOptions.EndPoints.Add("localhost");
/// ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(configurationOptions);
/// </code>
/// If the connection multiplexer timeout expires, a <c>StackExchange.Redis.RedisTimeoutException</c> will be
/// thrown.
/// <p/>
/// This is an extension method added to the <see cref="IDatabase"/> class, for convenience.
/// </summary>
/// <param name="db">The <see cref="IDatabase"/> class where this extension method is applied.</param>
/// <param name="timeout">Server-side timeout for the wait. A value of <c>0</c> means to wait indefinitely.</param>
/// <param name="keys">The keys to check.</param>
/// <param name="minMaxModifier">Specify from which end of the sorted set to pop values. If set to <c>MinMaxModifier.Min</c>
/// then the minimum elements will be popped, otherwise the maximum values.</param>
/// <param name="count">The maximum number of records to pop out. If set to <c>null</c> then the server default
/// will be used.</param>
/// <returns>A collection of sorted set entries paired with their scores, together with the key they were popped
/// from, or <c>null</c> if the server timeout expires.</returns>
/// <remarks><seealso href="https://redis.io/commands/bzmpop"/></remarks>
public static Tuple<RedisKey, List<RedisValueWithScore>>? BzmPop(this IDatabase db, int timeout, RedisKey[] keys, MinMaxModifier minMaxModifier, long? count = null)
{
    var command = CoreCommandBuilder.BzmPop(timeout, keys, minMaxModifier, count);
    return db.Execute(command).ToSortedSetPopResult();
}

/// <summary>
/// Syntactic sugar for <see cref="BzmPop(StackExchange.Redis.IDatabase,int,StackExchange.Redis.RedisKey[],StackExchange.Redis.Order,System.Nullable{long})"/>,
/// where only one key is used.
/// </summary>
/// <param name="db">The <see cref="IDatabase"/> class where this extension method is applied.</param>
/// <param name="timeout">Server-side timeout for the wait. A value of <c>0</c> means to wait indefinitely.</param>
/// <param name="key">The key to check.</param>
/// <param name="minMaxModifier">Specify from which end of the sorted set to pop values. If set to <c>MinMaxModifier.Min</c>
/// then the minimum elements will be popped, otherwise the maximum values.</param>
/// <param name="count">The maximum number of records to pop out. If set to <c>null</c> then the server default
/// will be used.</param>
/// <returns>A collection of sorted set entries paired with their scores, together with the key they were popped
/// from, or <c>null</c> if the server timeout expires.</returns>
/// <remarks><seealso href="https://redis.io/commands/bzmpop"/></remarks>
public static Tuple<RedisKey, List<RedisValueWithScore>>? BzmPop(this IDatabase db, int timeout, RedisKey key, MinMaxModifier minMaxModifier, long? count = null)
{
    return BzmPop(db, timeout, new[] { key }, minMaxModifier, count);
}
