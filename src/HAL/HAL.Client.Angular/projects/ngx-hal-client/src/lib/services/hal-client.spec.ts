import { TestBed } from '@angular/core/testing';
import { HttpHeaders, provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { beforeEach, afterEach, describe, expect, it } from 'vitest';
import { HalClient } from './hal-client';
import { Resource } from '../models/resource';
import { FormsResource } from '../models/formsResource';
import { ProblemDetails } from '../models/problem-details';
import { ListResource } from '../models/listResource';

describe('HalClient', () => {
  let client: HalClient;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    client = TestBed.inject(HalClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('getResource', () => {
    it('returns a Resource on a successful response', async () => {
      const promise = client.getResource<{ name: string }>('/items/1');

      const req = httpMock.expectOne('/items/1');
      expect(req.request.method).toBe('GET');
      expect(req.request.headers.get('Accept')).toBe('application/hal+json');
      req.flush({ _links: { self: [{ href: '/items/1' }] }, name: 'Item 1' });

      const response = await promise;

      expect(response.ok).toBe(true);
      expect(response.body).toBeInstanceOf(Resource);
      expect((response.body as Resource & { name: string }).name).toBe('Item 1');
    });

    it('returns a ProblemDetails when the server returns an error with problem-details body', async () => {
      const promise = client.getResource('/items/1');

      const req = httpMock.expectOne('/items/1');
      req.flush(
        { _links: { self: [{ href: '/items/1' }] }, status: 404, title: 'Not Found', detail: 'No such item.' },
        { status: 404, statusText: 'Not Found' }
      );

      const response = await promise;

      expect(response.ok).toBe(false);
      expect(response.body).toBeInstanceOf(ProblemDetails);
      expect((response.body as ProblemDetails).status).toBe(404);
      expect((response.body as ProblemDetails).title).toBe('Not Found');
    });

    it('synthesizes a ProblemDetails when the server returns an error without a problem-details body', async () => {
      const promise = client.getResource('/items/1');

      const req = httpMock.expectOne('/items/1');
      req.flush('Internal error', { status: 500, statusText: 'Internal Server Error' });

      const response = await promise;

      expect(response.ok).toBe(false);
      expect(response.body).toBeInstanceOf(ProblemDetails);
      expect((response.body as ProblemDetails).status).toBe(500);
      expect((response.body as ProblemDetails).title).toBe('Internal Server Error');
    });

    it('synthesizes a ProblemDetails when the server returns an empty successful response', async () => {
      const promise = client.getResource('/items/1');

      const req = httpMock.expectOne('/items/1');
      req.flush(null);

      const response = await promise;

      expect(response.body).toBeInstanceOf(ProblemDetails);
      expect((response.body as ProblemDetails).detail).toContain('empty response');
    });

    it('passes through custom headers and always adds the HAL accept header', async () => {
      const headers = new HttpHeaders({ 'X-Custom': 'value' });
      const promise = client.getResource('/items/1', headers);

      const req = httpMock.expectOne('/items/1');
      expect(req.request.headers.get('X-Custom')).toBe('value');
      expect(req.request.headers.get('Accept')).toBe('application/hal+json');
      req.flush({ _links: { self: [{ href: '/items/1' }] } });

      await promise;
    });
  });

  describe('getListResource', () => {
    it('returns a ListResource on a successful response', async () => {
      const promise = client.getListResource<{ name: string }>('/items');

      const req = httpMock.expectOne('/items');
      req.flush({
        _links: { self: [{ href: '/items' }] },
        _embedded: { items: [{ _links: { self: [{ href: '/items/1' }] }, name: 'Item 1' }] },
      });

      const response = await promise;

      expect(response.body).toBeInstanceOf(ListResource);
      expect((response.body as ListResource<{ name: string }>)._embedded.items[0].name).toBe('Item 1');
    });
  });

  describe('getFormsResource', () => {
    it('returns a FormsResource on a successful response', async () => {
      const promise = client.getFormsResource('/items/1/form');

      const req = httpMock.expectOne('/items/1/form');
      req.flush({
        _links: { self: [{ href: '/items/1/form' }] },
        _templates: { default: { properties: [] } },
      });

      const response = await promise;

      expect(response.body).toBeInstanceOf(FormsResource);
      expect((response.body as unknown as FormsResource)._templates['default']).toBeDefined();
    });
  });

  describe('postAndGetResultAsResource', () => {
    it('sends the body and returns a Resource', async () => {
      const promise = client.postAndGetResultAsResource('/items', { name: 'New Item' });

      const req = httpMock.expectOne('/items');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ name: 'New Item' });
      req.flush({ _links: { self: [{ href: '/items/1' }] }, name: 'New Item' });

      const response = await promise;

      expect(response.body).toBeInstanceOf(Resource);
    });
  });

  describe('postAndGetResultAsListResource', () => {
    it('sends the body and returns a ListResource', async () => {
      const promise = client.postAndGetResultAsListResource('/items/search', { query: 'foo' });

      const req = httpMock.expectOne('/items/search');
      expect(req.request.method).toBe('POST');
      req.flush({ _links: { self: [{ href: '/items/search' }] }, _embedded: { items: [] } });

      const response = await promise;

      expect(response.body).toBeInstanceOf(ListResource);
    });
  });

  describe('postAndGetResultAsFormsResource', () => {
    it('sends the body and returns a FormsResource', async () => {
      const promise = client.postAndGetResultAsFormsResource('/items', { name: 'New Item' });

      const req = httpMock.expectOne('/items');
      req.flush({ _links: { self: [{ href: '/items/1' }] }, _templates: {} });

      const response = await promise;

      expect(response.body).toBeInstanceOf(FormsResource);
    });
  });

  describe('putAndGetResultAsResource', () => {
    it('sends a PUT request and returns a Resource', async () => {
      const promise = client.putAndGetResultAsResource('/items/1', { name: 'Updated Item' });

      const req = httpMock.expectOne('/items/1');
      expect(req.request.method).toBe('PUT');
      req.flush({ _links: { self: [{ href: '/items/1' }] }, name: 'Updated Item' });

      const response = await promise;

      expect(response.body).toBeInstanceOf(Resource);
      expect((response.body as Resource & { name: string }).name).toBe('Updated Item');
    });
  });

  describe('putAndGetResultAsListResource', () => {
    it('sends a PUT request and returns a ListResource', async () => {
      const promise = client.putAndGetResultAsListResource('/items', { items: [] });

      const req = httpMock.expectOne('/items');
      expect(req.request.method).toBe('PUT');
      req.flush({ _links: { self: [{ href: '/items' }] }, _embedded: { items: [] } });

      const response = await promise;

      expect(response.body).toBeInstanceOf(ListResource);
    });
  });

  describe('putAndGetResultAsFormsResource', () => {
    it('sends a PUT request and returns a FormsResource', async () => {
      const promise = client.putAndGetResultAsFormsResource('/items/1', { name: 'Updated Item' });

      const req = httpMock.expectOne('/items/1');
      expect(req.request.method).toBe('PUT');
      req.flush({ _links: { self: [{ href: '/items/1' }] }, _templates: {} });

      const response = await promise;

      expect(response.body).toBeInstanceOf(FormsResource);
    });
  });

  describe('delete', () => {
    it('sends a DELETE request and returns an empty ok response', async () => {
      const promise = client.delete('/items/1');

      const req = httpMock.expectOne('/items/1');
      expect(req.request.method).toBe('DELETE');
      req.flush(null, { status: 204, statusText: 'No Content' });

      const response = await promise;

      expect(response.ok).toBe(true);
      expect(response.body).toBeNull();
    });

    it('returns a ProblemDetails when the delete fails', async () => {
      const promise = client.delete('/items/1');

      const req = httpMock.expectOne('/items/1');
      req.flush(
        { _links: { self: [{ href: '/items/1' }] }, status: 404, title: 'Not Found' },
        { status: 404, statusText: 'Not Found' }
      );

      const response = await promise;

      expect(response.ok).toBe(false);
      expect(response.body).toBeInstanceOf(ProblemDetails);
      expect((response.body as ProblemDetails).status).toBe(404);
    });
  });

  describe('httpClient getter', () => {
    it('exposes the underlying HttpClient instance', () => {
      expect(client.httpClient).toBeDefined();
    });
  });
});
