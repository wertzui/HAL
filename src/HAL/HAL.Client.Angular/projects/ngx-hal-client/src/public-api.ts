/*
 * Public API Surface of ngx-hal-client
 */

export * from './lib/models/formsResource';
export * from './lib/models/link';
export * from './lib/models/listResource';
export * from './lib/models/page';
export * from './lib/models/pagedListFormsResource';
export * from './lib/models/pagedListResource';
export * from './lib/models/problem-details';
export * from './lib/models/resource';
export * from './lib/models/resourceOf';

export * from './lib/services/form.service';
export * from './lib/services/hal-client';
export * from './lib/services/resource-factory';

export { provideHalClient, ensureJsonPreservesTimeZoneInformation, restoreDefaultToJson } from './lib/provideHalClient';