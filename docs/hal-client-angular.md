# @wertzui/ngx-hal-client

An Angular client library for consuming HAL (Hypertext Application Language) APIs.

## Installation

```sh
npm install @wertzui/ngx-hal-client
```

## Setup

Register the HAL client providers in your `app.config.ts`:

```typescript
import { ApplicationConfig } from '@angular/core';
import { provideHalClient } from '@wertzui/ngx-hal-client';

export const appConfig: ApplicationConfig = {
  providers: [
    provideHalClient(),
    // other providers...
  ]
};
```

`provideHalClient()` registers two services:

- **`HalClient`** – makes HTTP requests and deserializes HAL responses.
- **`FormService`** / **`SignalFormService`** – creates Angular reactive forms and Signal Forms from HAL-Forms templates.

It also overrides `Date.prototype.toJSON` so that serialized dates include timezone information. If you ever need to revert that, call `restoreDefaultToJson()`.

## Core models

| Class / Interface | Description |
|-------------------|-------------|
| `Resource` | Base HAL resource with `_links` and `_embedded`. |
| `Resource & TDto` | A resource whose properties are the intersection of `Resource` and your DTO type. |
| `ListResource<TListEntryDto>` | A resource whose `_embedded.items` array holds typed entry resources. |
| `FormsResource` | A resource that additionally carries a `_templates` map for HAL-Forms. |
| `PagedListResource<T>` | A `ListResource` that also exposes paging information via `Page`. |
| `Link` | Represents a single HAL link (`href`, optional `name`, `templated`, etc.). |
| `ProblemDetails` | RFC 7807 error response. |

## Making requests with `HalClient`

Inject `HalClient` into any service or component. Every method is `async` and returns `Promise<HttpResponse<Resource | ProblemDetails>>`. Check `response.ok` before accessing `response.body`.

### GET a single resource

```typescript
import { Injectable } from '@angular/core';
import { HalClient, Resource } from '@wertzui/ngx-hal-client';

interface ItemDto {
  id: number;
  name: string;
}

@Injectable({ providedIn: 'root' })
export class ItemService {
  constructor(private readonly halClient: HalClient) {}

  async getItem(uri: string): Promise<ItemDto | undefined> {
    const response = await this.halClient.getResource<ItemDto>(uri);

    if (!response.ok) {
      console.error('Failed to load item', response.body);
      return undefined;
    }

    // response.body is Resource & ItemDto
    return response.body;
  }
}
```

### GET a list resource

Use `getListResource` when the endpoint returns an embedded array of items:

```typescript
async getItems(uri: string) {
  const response = await this.halClient.getListResource<ItemDto>(uri);

  if (!response.ok) {
    console.error('Failed to load items', response.body);
    return [];
  }

  // response.body is ListResource<ItemDto & ResourceDto>
  // Items are in response.body._embedded.items
  return response.body._embedded.items;
}
```

### GET a forms resource

Use `getFormsResource` when the endpoint returns a HAL-Forms resource:

```typescript
async getForm(uri: string) {
  const response = await this.halClient.getFormsResource(uri);

  if (!response.ok) {
    console.error('Failed to load form', response.body);
    return null;
  }

  // response.body._templates contains the HAL-Forms templates
  return response.body._templates;
}
```

### POST

```typescript
async createItem(uri: string, dto: CreateItemDto) {
  const response = await this.halClient.postAndGetResultAsResource<ItemDto>(uri, dto);

  if (!response.ok) {
    console.error('Create failed', response.body);
    return null;
  }

  return response.body; // Resource & ItemDto
}
```

### PUT

```typescript
async updateItem(uri: string, dto: UpdateItemDto) {
  const response = await this.halClient.putAndGetResultAsResource<ItemDto>(uri, dto);

  if (!response.ok) {
    console.error('Update failed', response.body);
    return null;
  }

  return response.body;
}
```

### DELETE

```typescript
async deleteItem(uri: string) {
  const response = await this.halClient.deleteAndGetResultAsResource<ItemDto>(uri);

  if (!response.ok) {
    console.error('Delete failed', response.body);
  }
}
```

### Using `get` / `post` / `put` / `delete` with a custom factory

The convenience methods (`getResource`, `getListResource`, …) delegate to lower-level `get` / `post` / `put` / `delete` methods that accept an explicit `factory` function. Use these when you need a resource type that does not fit the built-in helpers:

```typescript
import { ResourceFactory } from '@wertzui/ngx-hal-client';

const response = await this.halClient.get<ItemDto, Resource & ItemDto>(
  uri,
  ResourceFactory.createResource
);
```

## Navigating links

Every `Resource` exposes `_links` as a map from relation name to `Link[]`. Use the `self` link to reload a resource, or follow `next`/`prev` links for pagination:

```typescript
const listResponse = await this.halClient.getListResource<ItemDto>(uri);
if (!listResponse.ok) return;

const nextLinks = listResponse.body._links['next'];
if (nextLinks?.length) {
  const nextPage = await this.halClient.getListResource<ItemDto>(nextLinks[0].href);
}
```

## Building reactive forms with `FormService`

`FormService` creates Angular `ReactiveFormsModule` form groups directly from HAL-Forms templates.

```typescript
import { Component, OnInit } from '@angular/core';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { HalClient, FormService } from '@wertzui/ngx-hal-client';

@Component({
  selector: 'app-item-form',
  standalone: true,
  imports: [ReactiveFormsModule],
  template: `
    <form [formGroup]="form" (ngSubmit)="onSubmit()">
      <input formControlName="name" />
      <button type="submit">Save</button>
    </form>
  `
})
export class ItemFormComponent implements OnInit {
  form!: FormGroup;

  constructor(
    private readonly halClient: HalClient,
    private readonly formService: FormService
  ) {}

  async ngOnInit() {
    const response = await this.halClient.getFormsResource('/items/new');
    if (!response.ok) return;

    const formGroups = this.formService.createFormGroupsFromTemplates(
      response.body._templates
    );
    // Use the "default" template by convention
    this.form = formGroups['default'];
  }

  onSubmit() {
    console.log(this.form.value);
  }
}
```

### `FormService` API

| Method | Description |
|--------|-------------|
| `createFormGroupsFromTemplates(templates)` | Returns an object mapping template names to `FormGroup` instances. |
| `createFormGroupFromTemplate(template)` | Creates a single `FormGroup` from one template. |
| `createFormArrayFromTemplates(templates, ignoredProperties?)` | Creates a `FormArray` from numbered templates (for collection properties). |
| `createFormControl(property)` | Creates a single `FormControl`, `FormGroup` (for Object), or `FormArray` (for Collection) from a HAL-Forms property. Applies all validators declared in the property (`required`, `min`, `max`, `minLength`, `maxLength`, `regex`, `email`). |

## Building Signal Forms with `SignalFormService`

`SignalFormService` is the [Signal Forms](https://angular.dev/guide/forms/signal-forms) equivalent of `FormService`.

```typescript
import { Component, OnInit } from '@angular/core';
import { HalClient, SignalFormService, SignalForm } from '@wertzui/ngx-hal-client';

@Component({ selector: 'app-item-form', standalone: true, template: '' })
export class ItemFormComponent implements OnInit {
  signalForm?: SignalForm;

  constructor(
    private readonly halClient: HalClient,
    private readonly signalFormService: SignalFormService
  ) {}

  async ngOnInit() {
    const response = await this.halClient.getFormsResource('/items/new');
    if (!response.ok) return;

    const signalForms = this.signalFormService.createSignalFormsFromTemplates(
      response.body._templates
    );
    this.signalForm = signalForms['default'];
    // this.signalForm.model() – the current model value
    // this.signalForm.form   – the field tree for [formField] bindings
  }
}
```

### Pre-populating with an existing model

Pass a model map to override the template default values:

```typescript
const signalForms = this.signalFormService.createSignalFormsFromTemplates(
  templates,
  { default: { name: 'Existing Name' } }
);
```

### `SignalFormService` API

| Method | Description |
|--------|-------------|
| `createSignalFormsFromTemplates(templates, model?)` | Returns an object mapping template names to `SignalForm` instances, optionally pre-populated from `model`. |
| `createSignalFormFromTemplate(template, model?)` | Creates a single `SignalForm`, optionally pre-populated. |
| `createSignalFormArrayFromTemplates(templates, ignoredProperties?, model?)` | Creates an array of `SignalForm` instances for collection-type properties. |

## Timezone-aware date serialization

By default, `provideHalClient()` patches `Date.prototype.toJSON` so that dates serialized to JSON include the local timezone offset (instead of always converting to UTC). This ensures round-trip fidelity for date/time values.

```typescript
// The patch is applied automatically by provideHalClient().
// To revert it (e.g., in tests):
import { restoreDefaultToJson } from '@wertzui/ngx-hal-client';
restoreDefaultToJson();
```
