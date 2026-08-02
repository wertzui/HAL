import { describe, expect, it } from 'vitest';
import { ResourceFactory } from './resource-factory';
import { Resource } from '../models/resource';
import { FormsResource } from '../models/formsResource';
import { ProblemDetails } from '../models/problem-details';
import { ListResource } from '../models/listResource';
import { PagedListResource } from '../models/pagedListResource';
import { PagedListFormsResource } from '../models/pagedListFormsResource';

describe('ResourceFactory', () => {
  it('createResource builds a Resource from the dto', () => {
    const resource = ResourceFactory.createResource({
      _links: { self: [{ href: '/items/1' }] },
      name: 'Item 1',
    });

    expect(resource).toBeInstanceOf(Resource);
    expect(resource.name).toBe('Item 1');
  });

  it('createFormResource builds a FormsResource from the dto', () => {
    const resource = ResourceFactory.createFormResource({
      _links: { self: [{ href: '/items/1' }] },
      _templates: { default: { properties: [] } },
    });

    expect(resource).toBeInstanceOf(FormsResource);
    expect(resource._templates['default']).toBeDefined();
  });

  it('createProblemDetails builds a ProblemDetails from the dto', () => {
    const resource = ResourceFactory.createProblemDetails({
      _links: { self: [{ href: '/errors/1' }] },
      status: 404,
      title: 'Not Found',
    });

    expect(resource).toBeInstanceOf(ProblemDetails);
    expect(resource.status).toBe(404);
  });

  it('createListResource builds a ListResource from the dto', () => {
    const resource = ResourceFactory.createListResource<{ name: string }, object>({
      _links: { self: [{ href: '/items' }] },
      _embedded: { items: [{ _links: { self: [{ href: '/items/1' }] }, name: 'Item 1' }] as ({ name: string } & { _links: { self: { href: string }[] } })[] },
    });

    expect(resource).toBeInstanceOf(ListResource);
    expect(resource._embedded.items[0].name).toBe('Item 1');
  });

  it('createPagedListResource builds a PagedListResource from the dto', () => {
    const resource = ResourceFactory.createPagedListResource<{ name: string }, object>({
      _links: { self: [{ href: '/items?page=1' }] },
      currentPage: 1,
      totalPages: 3,
    });

    expect(resource).toBeInstanceOf(PagedListResource);
    expect(resource.currentPage).toBe(1);
    expect(resource.totalPages).toBe(3);
  });

  it('createPagedListFormsResource builds a PagedListFormsResource from the dto', () => {
    const resource = ResourceFactory.createPagedListFormsResource<{ name: string }, object>({
      _links: { self: [{ href: '/items?page=1' }] },
      currentPage: 1,
      totalPages: 3,
      _templates: { search: { properties: [] } },
    });

    expect(resource).toBeInstanceOf(PagedListFormsResource);
    expect(resource._templates['search']).toBeDefined();
  });
});
