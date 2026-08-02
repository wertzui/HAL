import { describe, expect, it } from 'vitest';
import { PagedListResource, PagedListResourceDto } from './pagedListResource';

interface ItemDto {
  name: string;
}

describe('PagedListResource', () => {
  it('exposes currentPage and totalPages from the dto', () => {
    const dto: PagedListResourceDto<ItemDto> = {
      _links: { self: [{ href: '/items?page=1' }] },
      currentPage: 1,
      totalPages: 5,
    };

    const resource = new PagedListResource(dto);

    expect(resource.currentPage).toBe(1);
    expect(resource.totalPages).toBe(5);
  });

  it('exposes the paging links (first, prev, next, last) as Link instances', () => {
    const dto: PagedListResourceDto<ItemDto> = {
      _links: {
        self: [{ href: '/items?page=2' }],
        first: [{ href: '/items?page=1' }],
        prev: [{ href: '/items?page=1' }],
        next: [{ href: '/items?page=3' }],
        last: [{ href: '/items?page=5' }],
      },
      currentPage: 2,
      totalPages: 5,
    };

    const resource = new PagedListResource(dto);

    expect(resource._links.first?.[0].href).toBe('/items?page=1');
    expect(resource._links.prev?.[0].href).toBe('/items?page=1');
    expect(resource._links.next?.[0].href).toBe('/items?page=3');
    expect(resource._links.last?.[0].href).toBe('/items?page=5');
  });

  it('exposes the embedded items as before', () => {
    const dto: PagedListResourceDto<ItemDto> = {
      _links: { self: [{ href: '/items?page=1' }] },
      _embedded: {
        items: [{ _links: { self: [{ href: '/items/1' }] }, name: 'Item 1' }] as (ItemDto & { _links: { self: { href: string }[] } })[],
      },
      currentPage: 1,
      totalPages: 1,
    };

    const resource = new PagedListResource(dto);

    expect(resource._embedded.items).toHaveLength(1);
    expect(resource._embedded.items[0].name).toBe('Item 1');
  });
});
