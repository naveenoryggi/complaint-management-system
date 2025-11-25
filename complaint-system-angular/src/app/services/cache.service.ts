import { Injectable } from '@angular/core';
import { Observable, BehaviorSubject, of, timer, from } from 'rxjs';
import { map, tap, shareReplay, takeWhile, distinctUntilChanged } from 'rxjs/operators';

export interface CacheEntry<T> {
  data: T;
  timestamp: number;
  ttl: number;
  key: string;
}

export interface CacheConfig {
  ttl: number; // Time to live in milliseconds
  maxSize: number; // Maximum number of entries in cache
  enableRefreshAhead: boolean; // Pre-refresh data before expiry
  refreshAheadThreshold: number; // Refresh when this much time remains (percentage)
}

export interface CacheGroup {
  name: string;
  config: CacheConfig;
}

@Injectable({
  providedIn: 'root'
})
export class CacheService {
  private cache = new Map<string, CacheEntry<any>>();
  private subjects = new Map<string, BehaviorSubject<any>>();
  private refreshTimers = new Map<string, any>();
  private cacheGroups = new Map<string, CacheConfig>();

  // Default cache configurations
  private readonly defaultConfigs: { [key: string]: CacheConfig } = {
    dashboard: {
      ttl: 5 * 60 * 1000, // 5 minutes
      maxSize: 50,
      enableRefreshAhead: true,
      refreshAheadThreshold: 0.8 // Refresh when 80% of TTL has passed
    },
    masterData: {
      ttl: 30 * 60 * 1000, // 30 minutes
      maxSize: 100,
      enableRefreshAhead: false,
      refreshAheadThreshold: 0.9
    },
    userPreferences: {
      ttl: 15 * 60 * 1000, // 15 minutes
      maxSize: 20,
      enableRefreshAhead: true,
      refreshAheadThreshold: 0.85
    },
    complaints: {
      ttl: 2 * 60 * 1000, // 2 minutes
      maxSize: 200,
      enableRefreshAhead: true,
      refreshAheadThreshold: 0.75
    },
    referenceData: {
      ttl: 60 * 60 * 1000, // 1 hour
      maxSize: 500,
      enableRefreshAhead: false,
      refreshAheadThreshold: 0.95
    }
  };

  constructor() {
    this.initializeCacheGroups();
    this.setupPeriodicCleanup();
  }

  /**
   * Initialize cache groups with default configurations
   */
  private initializeCacheGroups(): void {
    Object.keys(this.defaultConfigs).forEach(groupName => {
      this.cacheGroups.set(groupName, this.defaultConfigs[groupName]);
    });
  }

  /**
   * Setup periodic cleanup of expired cache entries
   */
  private setupPeriodicCleanup(): void {
    timer(0, 60 * 1000) // Check every minute
      .pipe(takeWhile(() => true))
      .subscribe(() => this.cleanupExpiredEntries());
  }

  /**
   * Get cached data or fetch new data if not available or expired
   */
  get<T>(
    key: string,
    fetchFn: () => Observable<T>,
    groupName: string = 'dashboard',
    customTTL?: number
  ): Observable<T> {
    const config = this.getCacheConfig(groupName);
    const ttl = customTTL || config.ttl;
    const cacheKey = `${groupName}:${key}`;

    // Check if data exists and is valid
    const cachedEntry = this.cache.get(cacheKey);
    if (cachedEntry && this.isEntryValid(cachedEntry)) {

      // Setup refresh ahead if enabled
      if (config.enableRefreshAhead && this.shouldRefreshAhead(cachedEntry, config)) {
        this.setupRefreshAhead(cacheKey, fetchFn, groupName, ttl);
      }

      return of(cachedEntry.data);
    }

    // Return existing subject if in flight
    if (this.subjects.has(cacheKey)) {
      return this.subjects.get(cacheKey)!.asObservable();
    }

    // Create new subject for this request
    const subject = new BehaviorSubject<T | null>(null);
    this.subjects.set(cacheKey, subject);

    // Fetch new data
    fetchFn().subscribe({
      next: (data) => {
        this.set(cacheKey, data, groupName, ttl);
        subject.next(data);
        subject.complete();
        this.subjects.delete(cacheKey);
      },
      error: (error) => {
        subject.error(error);
        this.subjects.delete(cacheKey);
      }
    });

    return subject.asObservable().pipe(
      map(result => result as T)
    );
  }

  /**
   * Set data in cache
   */
  set<T>(
    key: string,
    data: T,
    groupName: string = 'dashboard',
    customTTL?: number
  ): void {
    const config = this.getCacheConfig(groupName);
    const ttl = customTTL || config.ttl;
    const cacheKey = `${groupName}:${key}`;

    // Enforce max size
    this.enforceMaxSize(groupName);

    const entry: CacheEntry<T> = {
      data,
      timestamp: Date.now(),
      ttl,
      key: cacheKey
    };

    this.cache.set(cacheKey, entry);

    // Setup refresh ahead if enabled
    if (config.enableRefreshAhead) {
      this.setupRefreshAhead(cacheKey, () => of(data), groupName, ttl);
    }
  }

  /**
   * Get data without fetching (returns null if not available)
   */
  getImmediate<T>(key: string, groupName: string = 'dashboard'): T | null {
    const cacheKey = `${groupName}:${key}`;
    const entry = this.cache.get(cacheKey);

    if (entry && this.isEntryValid(entry)) {
      return entry.data;
    }

    return null;
  }

  /**
   * Invalidate specific cache entry
   */
  invalidate(key: string, groupName: string = 'dashboard'): void {
    const cacheKey = `${groupName}:${key}`;
    this.cache.delete(cacheKey);
    this.clearRefreshTimer(cacheKey);

    if (this.subjects.has(cacheKey)) {
      this.subjects.get(cacheKey)!.complete();
      this.subjects.delete(cacheKey);
    }
  }

  /**
   * Invalidate entire cache group
   */
  invalidateGroup(groupName: string): void {
    const keysToDelete: string[] = [];

    this.cache.forEach((entry, key) => {
      if (key.startsWith(`${groupName}:`)) {
        keysToDelete.push(key);
      }
    });

    keysToDelete.forEach(key => {
      this.cache.delete(key);
      this.clearRefreshTimer(key);

      if (this.subjects.has(key)) {
        this.subjects.get(key)!.complete();
        this.subjects.delete(key);
      }
    });
  }

  /**
   * Clear all cache
   */
  clearAll(): void {
    this.cache.clear();
    this.subjects.forEach(subject => subject.complete());
    this.subjects.clear();
    this.refreshTimers.forEach(timer => clearTimeout(timer));
    this.refreshTimers.clear();
  }

  /**
   * Get cache statistics
   */
  getStats(): {
    totalEntries: number;
    groupStats: { [groupName: string]: { count: number; hitRate: number } };
    memoryUsage: number;
  } {
    const groupStats: { [groupName: string]: { count: number; hitRate: number } } = {};
    let totalEntries = 0;

    this.cache.forEach((entry, key) => {
      const groupName = key.split(':')[0];
      if (!groupStats[groupName]) {
        groupStats[groupName] = { count: 0, hitRate: 0 };
      }
      groupStats[groupName].count++;
      totalEntries++;
    });

    return {
      totalEntries,
      groupStats,
      memoryUsage: this.estimateMemoryUsage()
    };
  }

  /**
   * Create a cached observable with automatic refresh
   */
  createCachedObservable<T>(
    key: string,
    fetchFn: () => Observable<T>,
    groupName: string = 'dashboard',
    customTTL?: number
  ): Observable<T> {
    return this.get(key, fetchFn, groupName, customTTL).pipe(
      shareReplay(1),
      distinctUntilChanged()
    );
  }

  /**
   * Preload data into cache
   */
  preload<T>(
    key: string,
    fetchFn: () => Observable<T>,
    groupName: string = 'dashboard'
  ): Observable<T> {
    return new Observable<T>(subscriber => {
      fetchFn().subscribe({
        next: (data) => {
          this.set(key, data, groupName);
          subscriber.next(data);
          subscriber.complete();
        },
        error: (error) => subscriber.error(error)
      });
    });
  }

  /**
   * Get cache configuration for a group
   */
  private getCacheConfig(groupName: string): CacheConfig {
    return this.cacheGroups.get(groupName) || this.defaultConfigs['dashboard'];
  }

  /**
   * Check if cache entry is still valid
   */
  private isEntryValid<T>(entry: CacheEntry<T>): boolean {
    return Date.now() - entry.timestamp < entry.ttl;
  }

  /**
   * Check if entry should be refreshed ahead of expiry
   */
  private shouldRefreshAhead<T>(entry: CacheEntry<T>, config: CacheConfig): boolean {
    const elapsed = Date.now() - entry.timestamp;
    const threshold = entry.ttl * config.refreshAheadThreshold;
    return elapsed >= threshold;
  }

  /**
   * Setup refresh ahead timer
   */
  private setupRefreshAhead<T>(
    key: string,
    fetchFn: () => Observable<T>,
    groupName: string,
    ttl: number
  ): void {
    this.clearRefreshTimer(key);

    const config = this.getCacheConfig(groupName);
    const refreshDelay = ttl * config.refreshAheadThreshold;

    const timer = setTimeout(() => {
      fetchFn().subscribe({
        next: (data) => {
          this.cache.set(key, {
            data,
            timestamp: Date.now(),
            ttl,
            key
          });
        },
        error: () => {
          // Silent fail for refresh ahead
          console.warn(`Cache refresh failed for key: ${key}`);
        }
      });
    }, refreshDelay);

    this.refreshTimers.set(key, timer);
  }

  /**
   * Clear refresh timer for a key
   */
  private clearRefreshTimer(key: string): void {
    if (this.refreshTimers.has(key)) {
      clearTimeout(this.refreshTimers.get(key));
      this.refreshTimers.delete(key);
    }
  }

  /**
   * Enforce maximum cache size for a group
   */
  private enforceMaxSize(groupName: string): void {
    const config = this.getCacheConfig(groupName);
    const groupEntries: string[] = [];

    this.cache.forEach((entry, key) => {
      if (key.startsWith(`${groupName}:`)) {
        groupEntries.push(key);
      }
    });

    if (groupEntries.length >= config.maxSize) {
      // Remove oldest entries
      const sortedEntries = groupEntries
        .map(key => ({ key, entry: this.cache.get(key)! }))
        .sort((a, b) => a.entry.timestamp - b.entry.timestamp);

      const toRemove = sortedEntries.slice(0, groupEntries.length - config.maxSize + 1);
      toRemove.forEach(({ key }) => {
        this.cache.delete(key);
        this.clearRefreshTimer(key);
      });
    }
  }

  /**
   * Clean up expired entries
   */
  private cleanupExpiredEntries(): void {
    const now = Date.now();
    const expiredKeys: string[] = [];

    this.cache.forEach((entry, key) => {
      if (now - entry.timestamp >= entry.ttl) {
        expiredKeys.push(key);
      }
    });

    expiredKeys.forEach(key => {
      this.cache.delete(key);
      this.clearRefreshTimer(key);
    });
  }

  /**
   * Estimate memory usage of cache (rough calculation)
   */
  private estimateMemoryUsage(): number {
    let totalSize = 0;

    this.cache.forEach((entry) => {
      // Rough estimation: each entry takes ~200 bytes + data size
      if (entry && entry.data !== null && entry.data !== undefined) {
        try {
          totalSize += 200 + JSON.stringify(entry.data).length * 2;
        } catch (e) {
          // If data can't be stringified, use a default size
          totalSize += 200;
        }
      } else {
        totalSize += 200; // Just metadata size if no data
      }
    });

    return totalSize;
  }

  /**
   * Configure cache group settings
   */
  configureGroup(groupName: string, config: Partial<CacheConfig>): void {
    const currentConfig = this.getCacheConfig(groupName);
    this.cacheGroups.set(groupName, { ...currentConfig, ...config });
  }
}