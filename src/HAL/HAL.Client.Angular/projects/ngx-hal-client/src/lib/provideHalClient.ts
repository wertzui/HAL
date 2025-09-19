import { EnvironmentProviders, makeEnvironmentProviders } from '@angular/core';
import { HalClient } from './services/hal-client';
import { FormService } from '../public-api';

// Store the default toJSON method for potential restoration
const _defaultToJson = Date.prototype.toJSON;

/**
 * Ensures that the JSON representation of dates preserves the timezone information.
 * Overrides the toJSON method of the Date prototype.
 * This function is automatically called when providing the HAL client.
 */
export function ensureJsonPreservesTimeZoneInformation(): void {
  Date.prototype.toJSON = function () {
    let offSetMins = this.getTimezoneOffset();
    const msInMin = 6e4;
    const date = new Date(this.getTime() - offSetMins * msInMin);
    const stringDate = date.toISOString().slice(0, -1);
    const isNegative = offSetMins < 0
    if (isNegative) 
      offSetMins = offSetMins * -1
    const hours = offSetMins / 60
    const hoursStr = (hours - hours % 1).toString()
    const minsStr = (offSetMins % 60).toString()
    const padTime = (n: string) => n.length == 1 ? '0' + n : n
    const stringOffset = `${isNegative || offSetMins == 0 ? '+' : '-'}${padTime(hoursStr)}:${padTime(minsStr)}`
    return stringDate + stringOffset;
  }
}

/**
 * Restores the default toJSON method of the Date prototype.
 * Normally this is not needed, but you can use it if you want to revert the changes made by `ensureJsonPreservesTimeZoneInformation`.
 */
export function restoreDefaultToJson(): void {
  Date.prototype.toJSON = _defaultToJson;
}

/**
 * Provides the HalClient and FormService as injectable services.
 * It also ensures that the JSON representation of dates preserves the timezone information.
 * 
 * @returns An array of providers for the HAL client
 * 
 * @example
 * ```typescript
 * // In your app.config.ts
 * export const appConfig: ApplicationConfig = {
 *   providers: [
 *     provideHalClient(),
 *     // other providers...
 *   ]
 * };
 * ```
 */
export function provideHalClient(): EnvironmentProviders {
  ensureJsonPreservesTimeZoneInformation();
  
  return makeEnvironmentProviders([
    HalClient,
    FormService
  ]);
}
