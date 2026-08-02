import { describe, expect, it } from 'vitest';
import { ListResource, ListResourceDto } from './listResource';
import { Resource } from './resource';

interface ItemDto {
  name: string;
}

describe('ListResource', () => {
  it('exposes the embedded items as Resource instances', () => {
    const dto: ListResourceDto<ItemDto> = {
      _links: { self: [{ href: '/items' }] },
      _embedded: {
        items: [
          { _links: { self: [{ href: '/items/1' }] }, name: 'Item 1' },
          { _links: { self: [{ href: '/items/2' }] }, name: 'Item 2' },
        ] as (ItemDto & { _links: { self: { href: string }[] } })[],
      },
    };

    const resource = new ListResource(dto);

    expect(resource._embedded.items).toHaveLength(2);
    expect(resource._embedded.items[0]).toBeInstanceOf(Resource);
    expect(resource._embedded.items[0].name).toBe('Item 1');
    expect(resource._embedded.items[1].name).toBe('Item 2');
  });

  it('defaults items to an empty array when _embedded is missing entirely', () => {
    const dto: ListResourceDto<ItemDto> = {
      _links: { self: [{ href: '/items' }] },
    };

    const resource = new ListResource(dto);

    expect(resource._embedded.items).toEqual([]);
  });

  it('defaults items to an empty array when _embedded.items is missing', () => {
    const dto: ListResourceDto<ItemDto> = {
      _links: { self: [{ href: '/items' }] },
      _embedded: {} as ListResourceDto<ItemDto>['_embedded'],
    };

    const resource = new ListResource(dto);

    expect(resource._embedded.items).toEqual([]);
  });

  it('still throws when the self link is missing', () => {
    const dto = { _links: {} } as unknown as ListResourceDto<ItemDto>;

    expect(() => new ListResource(dto)).toThrow(/self link is missing/);
  });
});
