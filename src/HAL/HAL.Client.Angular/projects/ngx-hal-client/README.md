# Ngx-HAL-Client

[![npm version](https://badge.fury.io/js/@wertzui%2Fngx-hal-client.svg)](https://www.npmjs.com/package/@wertzui/ngx-hal-client)

## Introduction

Ngx-HAL-Client is a comprehensive Angular client library designed to seamlessly interact with RESTful APIs that implement the Hypertext Application Language (HAL) specification. This library simplifies the consumption of HAL-compliant endpoints by providing intuitive tools for navigating, retrieving, and manipulating hypermedia resources.

### Key Features

- **Full HAL Specification Support**: Handles HAL resources with `_links` and `_embedded` properties
- **Type-Safe API**: Strongly typed interfaces for resources and DTOs
- **Simplified Resource Navigation**: Easy traversal of API endpoints through links
- **Automatic Resource Parsing**: Converts HAL resources to TypeScript objects
- **Comprehensive Error Handling**: Built-in support for RFC 7807 Problem Details
- **Angular Integration**: Works with modern standalone components

## Installation

### Prerequisites

- Angular 20+

### Installing the Package

```bash
# npm
npm install @wertzui/ngx-hal-client --save

# or yarn
yarn add @wertzui/ngx-hal-client
```

### Setup

Add the HAL client to your application using the standalone component approach:

```typescript
// In app.config.ts
import { ApplicationConfig } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideHalClient } from '@wertzui/ngx-hal-client';

export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(),
    provideHalClient(),
    // other providers...
  ]
};
```

## Core Concepts

### HAL (Hypertext Application Language)

HAL is a format that provides a consistent way to hyperlink between resources in your API. HAL is a simple format that gives a consistent and easy way to hyperlink between resources in your API. Adopting HAL makes your API explorable and its documentation easily discoverable from within the API itself.

### Resources

In the HAL specification, a resource has:

- Regular properties containing the resource's state
- `_links` object containing links to related resources
- Optional `_embedded` object containing related resources

The `Resource` class in ngx-hal-client encapsulates this structure and provides helper methods to work with HAL resources.

### Links

Links in HAL represent relationships between resources. Each link has a relation type (or "rel") that describes its purpose. The `Link` class in ngx-hal-client provides a convenient interface to work with these links, allowing you to navigate from one resource to related resources.

### Embedded Resources

HAL allows resources to be embedded within other resources, reducing the need for multiple API calls. The `_embedded` property in a resource contains these embedded resources, which are fully-formed resources themselves.

### Problem Details

This library includes support for RFC 7807 Problem Details, which is a standardized format for error responses in HTTP APIs. The `ProblemDetails` class can be used to process error responses from the server.

## Usage Examples

### Basic Usage

```typescript
import { Injectable } from '@angular/core';
import { HalClient, Resource, ProblemDetails } from '@wertzui/ngx-hal-client';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private apiRoot = 'https://api.example.com';
  
  constructor(private halClient: HalClient) { }
  
  async getRootResource() {
    const response = await this.halClient.getResource<any>(this.apiRoot);
    
    if (response.body instanceof ProblemDetails) {
      throw new Error(`API error: ${response.body.title}`);
    }
    
    return response.body;
  }
}
```

### Navigating Between Resources

```typescript
import { Injectable } from '@angular/core';
import { HalClient, Resource, ProblemDetails } from '@wertzui/ngx-hal-client';

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  private apiRoot = 'https://api.example.com';
  
  constructor(private halClient: HalClient) { }
  
  async getOrderDetails(orderId: string) {
    // First, get the API root resource
    const rootResponse = await this.halClient.getResource<any>(this.apiRoot);
    if (rootResponse.body instanceof ProblemDetails) {
      throw new Error(`API error: ${rootResponse.body.title}`);
    }
    
    // Find the orders link
    const ordersLink = rootResponse.body.findLinks('orders')[0];
    if (!ordersLink) {
      throw new Error('Orders link not found');
    }
    
    // Get the orders collection
    const ordersResponse = await this.halClient.getListResource<OrderDto>(ordersLink.href);
    if (ordersResponse.body instanceof ProblemDetails) {
      throw new Error(`API error: ${ordersResponse.body.title}`);
    }
    
    // Find the specific order in embedded resources
    const order = ordersResponse.body._embedded['orders'].find(o => o.id === orderId);
    if (!order) {
      throw new Error(`Order ${orderId} not found`);
    }
    
    return order;
  }
}

interface OrderDto {
  id: string;
  customerName: string;
  total: number;
  date: Date;
}
```

### Creating a New Resource

```typescript
import { Injectable } from '@angular/core';
import { HalClient, Resource, ProblemDetails } from '@wertzui/ngx-hal-client';
import { HttpHeaders } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  constructor(private halClient: HalClient) { }
  
  async createProduct(rootResource: Resource, productData: NewProductDto) {
    // Find the products-collection link
    const productsLink = rootResource.findLinks('products-collection')[0];
    if (!productsLink) {
      throw new Error('Products collection link not found');
    }
    
    // Set headers for JSON content
    const headers = new HttpHeaders({
      'Content-Type': 'application/json'
    });
    
    // Post the new product data
    const response = await this.halClient.postAndGetResultAsResource<ProductDto>(
      productsLink.href,
      productData,
      headers
    );
    
    if (response.body instanceof ProblemDetails) {
      throw new Error(`Failed to create product: ${response.body.detail}`);
    }
    
    return response.body;
  }
}

interface NewProductDto {
  name: string;
  description: string;
  price: number;
  category: string;
}

interface ProductDto extends NewProductDto {
  id: string;
  createdAt: Date;
}
```

### Working with Forms Resources

```typescript
import { Injectable } from '@angular/core';
import { HalClient, FormsResource, ProblemDetails } from '@wertzui/ngx-hal-client';
import { FormService } from '@wertzui/ngx-hal-client';

@Injectable({
  providedIn: 'root'
})
export class UserRegistrationService {
  constructor(
    private halClient: HalClient,
    private formService: FormService
  ) { }
  
  async registerUser(registrationFormUri: string, userData: UserRegistrationDto) {
    // Get the registration form
    const formResponse = await this.halClient.getFormsResource<void>(registrationFormUri);
    if (formResponse.body instanceof ProblemDetails) {
      throw new Error(`Failed to get registration form: ${formResponse.body.title}`);
    }
    
    const formResource = formResponse.body as FormsResource;
    
    // Use FormService to submit the form with user data
    const response = await this.formService.submitForm<UserRegistrationDto, UserProfileDto>(
      formResource,
      'register',
      userData
    );
    
    if (response.body instanceof ProblemDetails) {
      throw new Error(`Registration failed: ${response.body.detail}`);
    }
    
    return response.body;
  }
}

interface UserRegistrationDto {
  username: string;
  email: string;
  password: string;
  confirmPassword: string;
}

interface UserProfileDto {
  id: string;
  username: string;
  email: string;
  createdAt: Date;
}
```

## API Reference

### HalClient

The main service for interacting with HAL-compliant APIs.

#### HalClient Methods

| Method | Description |
| ------ | ----------- |
| `getResource<TDto>(uri: string, headers?: HttpHeaders)` | Retrieves a resource from the specified URI |
| `getListResource<TListEntryDto>(uri: string, headers?: HttpHeaders)` | Retrieves a list resource from the specified URI |
| `getFormsResource<TDto = void>(uri: string, headers?: HttpHeaders)` | Retrieves a forms resource from the specified URI |
| `get<TDto, TResource extends Resource = Resource>(uri: string, factory: (dto: TDto & ResourceDto) => TResource & TDto, headers?: HttpHeaders)` | Sends a GET request to the specified URI |
| `postAndGetResultAsResource<TDto>(uri: string, body: any, headers?: HttpHeaders)` | Sends a POST request and returns the result as a resource |
| `postAndGetResultAsListResource<TListEntryDto>(uri: string, body: any, headers?: HttpHeaders)` | Sends a POST request and returns the result as a list resource |
| `postAndGetResultAsFormsResource<TDto = void>(uri: string, body: any, headers?: HttpHeaders)` | Sends a POST request and returns the result as a forms resource |
| `post<TDto, TResource extends Resource = Resource>(uri: string, body: any, factory: (dto: TDto & ResourceDto) => TResource & TDto, headers?: HttpHeaders)` | Sends a POST request to the specified URI |

### Resource

Represents a HAL resource with links and embedded resources.

#### Resource Properties

| Property | Type | Description |
| -------- | ---- | ----------- |
| `_links` | `{ [name: string]: Link[] \| undefined; self: Link[] }` | Links to related resources |
| `_embedded` | `{ [name: string]: Resource[] }` | Embedded resources |

#### Resource Methods

| Method | Description |
| ------ | ----------- |
| `findLinks(rel: string)` | Finds all links with the given relation type |
| `findLink(rel: string, name?: string)` | Finds a single link with the specified relation type and optional name |
| `findLinkWithType(rel: string, type: string)` | Finds a link with the specified relation type and media type |
| `getEmbedded<T extends Resource = Resource>(rel: string)` | Gets embedded resources with the specified relation type |

### Link

Represents a HAL link.

#### Properties

| Property | Type | Description |
| -------- | ---- | ----------- |
| `href` | `string` | URI of the linked resource |
| `templated` | `boolean` | Whether the link is templated (URI template) |
| `type` | `string \| undefined` | Media type of the linked resource |
| `deprecation` | `string \| undefined` | Deprecation warning |
| `name` | `string \| undefined` | Name of the link |
| `profile` | `string \| undefined` | Profile of the linked resource |
| `title` | `string \| undefined` | Title of the link |
| `hreflang` | `string \| undefined` | Language of the linked resource |

### FormService

Service for working with HAL Forms templates and Angular reactive forms. This service helps in creating form controls, form groups, and form arrays based on HAL form templates.

#### FormService Methods

| Method | Description |
| ------ | ----------- |
| `createFormGroupsFromTemplates(templates: Templates)` | Creates form groups from a collection of templates. Returns an object containing form groups created from the templates, with keys corresponding to template names. |
| `createFormArrayFromTemplates(templates: NumberTemplates, ignoredProperties?: string[])` | Creates a FormArray from the given templates, while ignoring the specified properties. Useful for handling collections of forms. |
| `createFormGroupFromTemplate<TProperties>(template: TemplateBase<string \| number, TProperties>)` | Creates a new FormGroup instance based on the provided template. The form group will contain form controls corresponding to the properties in the template. |
| `createFormControl<TValue, OptionsPromptField, OptionsValueField>(property: Property<TValue, OptionsPromptField, OptionsValueField>)` | Creates a form control based on the given property. Can return a FormGroup (for Object types), a FormArray (for Collection types), or a FormControl with appropriate validators. |

#### FormService Examples

##### Creating Form Groups from Templates

```typescript
import { Component, OnInit } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { HalClient, FormsResource, FormService, Templates, PropertyType } from '@wertzui/ngx-hal-client';

@Component({
  selector: 'app-user-form',
  templateUrl: './user-form.component.html'
})
export class UserFormComponent implements OnInit {
  public formGroups: { [key: string]: FormGroup } = {};
  private formResource: FormsResource | null = null;
  
  constructor(
    private halClient: HalClient,
    private formService: FormService
  ) { }
  
  async ngOnInit() {
    // Fetch a form resource from the server
    const response = await this.halClient.getFormsResource<void>('https://api.example.com/users/form');
    if (response.ok && !(response.body instanceof ProblemDetails)) {
      // Store the form resource for later use
      this.formResource = response.body as FormsResource;
      
      // Create form groups from the templates in the form resource
      this.formGroups = this.formService.createFormGroupsFromTemplates(this.formResource._templates);
      
      // Now you can access form groups by their template names
      // e.g., this.formGroups['default'], this.formGroups['admin'], etc.
    }
  }
  
  async submit() {
    if (this.formGroups['default'].valid && this.formResource) {
      const formData = this.formGroups['default'].value;
      
      // Find a template with target URI for submission
      const template = this.formResource._templates['default'];
      if (template && template.target) {
        // Submit the form data to the specified target
        const submitResponse = await this.halClient.postAndGetResultAsResource<UserProfileDto>(
          template.target,
          formData)
        );
          
          if (submitResponse.ok && !(submitResponse.body instanceof ProblemDetails)) {
            console.log('Form submitted successfully:', submitResponse.body);
            // Handle successful submission
          } else {
            console.error('Form submission failed:', submitResponse.body);
            // Handle submission error
          }
        }
      }
    }
  }
}
```

##### Working with Nested Forms and Collections

```typescript
import { Component } from '@angular/core';
import { FormArray, FormGroup } from '@angular/forms';
import { FormsResource, FormService, PropertyType } from '@wertzui/ngx-hal-client';

@Component({
  selector: 'app-complex-form',
  template: `
    <form [formGroup]="userForm" (ngSubmit)="onSubmit()">
      <div>
        <label>Name: <input formControlName="name"></label>
      </div>
      
      <!-- Address is a nested form group -->
      <div formGroupName="address">
        <label>Street: <input formControlName="street"></label>
        <label>City: <input formControlName="city"></label>
      </div>
      
      <!-- Phone numbers is a form array -->
      <div *ngFor="let phone of phoneNumbers.controls; let i = index">
        <label>Phone {{i+1}}: <input [formControl]="phone"></label>
      </div>
      
      <button type="submit" [disabled]="!userForm.valid">Submit</button>
    </form>
  `
})
export class ComplexFormComponent {
  userForm: FormGroup;
  
  constructor(private formService: FormService) {
    // Example of a complex form template structure. This is normally returned by your API
    const templates = {
      default: {
        title: "User Form",
        properties: [
          {
            name: "name",
            type: PropertyType.String,
            required: true,
            minLength: 2
          },
          {
            name: "address",
            type: PropertyType.Object,
            _templates: {
              default: {
                properties: [
                  { name: "street", type: PropertyType.String },
                  { name: "city", type: PropertyType.String, required: true }
                ]
              }
            }
          },
          {
            name: "phoneNumbers",
            type: PropertyType.Collection,
            _templates: {
              0: { properties: [{ name: "home", type: PropertyType.String }] },
              1: { properties: [{ name: "work", type: PropertyType.String }] },
              2: { properties: [{ name: "mobile", type: PropertyType.String }] }
            }
          }
        ]
      }
    };
    
    // Create form groups from templates
    const formGroups = this.formService.createFormGroupsFromTemplates(templates);
    this.userForm = formGroups['default'];
  }
  
  get phoneNumbers() {
    // Access the phone numbers form array
    return this.userForm.get('phoneNumbers') as FormArray;
  }
  
  onSubmit() {
    if (this.userForm.valid) {
      console.log(this.userForm.value);
      // Submit to server
    }
  }
}
```

##### Creating Form Controls with Validators

```typescript
import { Component } from '@angular/core';
import { FormService, Property, PropertyType } from '@wertzui/ngx-hal-client';
import { FormControl } from '@angular/forms';

@Component({
  selector: 'app-validator-example',
  template: `
    <input [formControl]="emailControl">
    <div *ngIf="emailControl.errors?.email">Invalid email format</div>
    <div *ngIf="emailControl.errors?.required">Email is required</div>
  `
})
export class ValidatorExampleComponent {
  emailControl: FormControl;
  
  constructor(private formService: FormService) {
    // Define a property with validators
    const emailProperty: Property<string, string, string> = {
      name: 'email',
      type: PropertyType.Email,
      required: true,
      value: '',
      minLength: 5,
      maxLength: 100
    };
    
    // Create a form control with validators
    this.emailControl = this.formService.createFormControl(emailProperty) as FormControl;
  }
}
```

These examples demonstrate how to use the FormService to create form controls, groups, and arrays from HAL Forms templates, handling nested forms, collections, and validators.

## Troubleshooting

### Common Issues and Solutions

#### "Self link is missing" Error

**Problem**: You receive an error saying "The self link is missing in the given ResourceDto".

**Solution**: Ensure your API is returning properly formatted HAL resources with a "self" link. Every HAL resource must have a "self" link that points to its own URI.

```json
{
  "_links": {
    "self": { "href": "/api/resource/123" }
  }
}
```

#### Type Conversion Issues

**Problem**: Properties like dates are not properly converted.

**Solution**: The library automatically converts ISO 8601 date strings to JavaScript Date objects. Ensure your API returns dates in ISO 8601 format. If you have custom date formats, you may need to handle the conversion manually.

#### Working with Templated Links

**Problem**: Unsure how to use templated links (URI templates).

**Solution**: Use the URI Templates standard to expand templated links. For example:

```typescript
import * as UriTemplate from 'uri-templates';

// For a templated link like "/users/{userId}"
const userLink = resource.findLink('user');
if (userLink && userLink.templated) {
  const template = new UriTemplate(userLink.href);
  const uri = template.fill({ userId: '123' });
  // Now use the expanded URI with the HAL client
  const response = await halClient.getResource<UserDto>(uri);
}
```

#### Error Handling Best Practices

Always check if the response is a ProblemDetails instance before proceeding:

```typescript
const response = await halClient.getResource<ResourceDto>(uri);
if (response.body instanceof ProblemDetails) {
  // Handle error based on status code
  switch (response.body.status) {
    case 404:
      console.error('Resource not found');
      break;
    case 401:
    case 403:
      console.error('Authentication/authorization error');
      break;
    default:
      console.error(`Error: ${response.body.title} - ${response.body.detail}`);
  }
  throw new Error('API request failed');
}

// Process successful response
const resource = response.body;
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License.
