import { Resource } from "./resource";

// Cannot use an interface here, see https://github.com/microsoft/TypeScript/issues/2225
export type ResourceOfDto<TState> = Resource & TState;