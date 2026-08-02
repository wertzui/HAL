import { afterEach, describe, expect, it } from 'vitest';
import { ensureJsonPreservesTimeZoneInformation, restoreDefaultToJson } from './provideHalClient';

describe('ensureJsonPreservesTimeZoneInformation / restoreDefaultToJson', () => {
  afterEach(() => {
    restoreDefaultToJson();
  });

  it('appends a timezone offset to the JSON representation of a Date', () => {
    ensureJsonPreservesTimeZoneInformation();

    const date = new Date('2024-01-15T13:45:00.000Z');
    const json = date.toJSON();

    // The date should still represent the same instant, with an explicit +hh:mm/-hh:mm offset
    // (rather than the default 'Z' suffix), reflecting the local timezone offset.
    expect(json).toMatch(/^2024-01-15T\d{2}:45:00\.000[+-]\d{2}:\d{2}$/);
  });

  it('produces a JSON date string that round-trips back to the same instant', () => {
    ensureJsonPreservesTimeZoneInformation();

    const original = new Date('2024-06-01T08:30:00.000Z');
    const json = original.toJSON();
    const roundTripped = new Date(json);

    expect(roundTripped.getTime()).toBe(original.getTime());
  });

  it('restores the default toJSON behavior (ISO string with Z suffix)', () => {
    ensureJsonPreservesTimeZoneInformation();
    restoreDefaultToJson();

    const date = new Date('2024-01-15T13:45:00.000Z');

    expect(date.toJSON()).toBe('2024-01-15T13:45:00.000Z');
  });
});
