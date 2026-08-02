import { describe, expect, it } from 'vitest';
import {
  FormsResource,
  FormsResourceDto,
  NumberTemplate,
  Options,
  Property,
  PropertyDto,
  PropertyType,
  Template,
  TemplateDto,
} from './formsResource';

function buildProperty(overrides: Partial<PropertyDto<any, string, string>> & { name: string; value: any }): PropertyDto<any, string, string> {
  return {
    type: PropertyType.Text,
    ...overrides,
  };
}

describe('Options', () => {
  it('assigns all given dto properties onto the instance', () => {
    const options = new Options({
      inline: [{ prompt: 'Red', value: 'red' }],
      maxItems: 2,
      minItems: 1,
      promptField: 'prompt',
      selectedValues: ['red'],
      valueField: 'value',
      link: { href: '/colors' },
    });

    expect(options.inline).toEqual([{ prompt: 'Red', value: 'red' }]);
    expect(options.maxItems).toBe(2);
    expect(options.minItems).toBe(1);
    expect(options.selectedValues).toEqual(['red']);
    expect(options.link).toEqual({ href: '/colors' });
  });

  it('defaults inline to an empty array when not given', () => {
    const options = new Options({ link: { href: '/colors' } });

    expect(options.inline).toEqual([]);
  });

  it('defaults inline to an empty array when constructed without a dto', () => {
    const options = new Options();

    expect(options.inline).toEqual([]);
  });
});

describe('Property', () => {
  it('assigns all given dto properties onto the instance', () => {
    const dto = buildProperty({
      name: 'age',
      value: 42,
      type: PropertyType.Number,
      min: 0,
      max: 120,
      required: true,
      prompt: 'Your age',
    });

    const property = new Property(dto);

    expect(property.name).toBe('age');
    expect(property.value).toBe(42);
    expect(property.type).toBe(PropertyType.Number);
    expect(property.min).toBe(0);
    expect(property.max).toBe(120);
    expect(property.required).toBe(true);
    expect(property.prompt).toBe('Your age');
  });

  it('defaults _templates to an empty object when the dto has none', () => {
    const property = new Property(buildProperty({ name: 'name', value: 'John' }));

    expect(property._templates).toEqual({});
  });

  it('builds Template instances from the dto _templates', () => {
    const dto = buildProperty({
      name: 'address',
      value: null,
      _templates: {
        default: { properties: [buildProperty({ name: 'street', value: '' })] },
      },
    });

    const property = new Property(dto);

    expect(property._templates['default']).toBeInstanceOf(Template);
    expect(property._templates['default']!.properties[0].name).toBe('street');
  });

  it('wraps a plain options dto into an Options instance', () => {
    const dto = buildProperty({
      name: 'color',
      value: null,
      options: { inline: [{ prompt: 'Red', value: 'red' }] },
    });

    const property = new Property(dto);

    expect(property.options).toBeInstanceOf(Options);
    expect(property.options!.inline).toEqual([{ prompt: 'Red', value: 'red' }]);
  });

  it('leaves options undefined when the dto has none', () => {
    const property = new Property(buildProperty({ name: 'name', value: 'John' }));

    expect(property.options).toBeUndefined();
  });
});

describe('Template', () => {
  it('maps every property dto into a Property instance', () => {
    const dto: TemplateDto = {
      properties: [
        buildProperty({ name: 'name', value: 'John' }),
        buildProperty({ name: 'age', type: PropertyType.Number, value: 30 }),
      ],
    };

    const template = new Template(dto);

    expect(template.properties).toHaveLength(2);
    expect(template.properties[0]).toBeInstanceOf(Property);
    expect(template.properties[0].name).toBe('name');
    expect(template.properties[1].name).toBe('age');
  });

  it('copies contentType, method, target and title from the dto', () => {
    const dto: TemplateDto = {
      contentType: 'application/json',
      method: 'POST',
      target: 'https://example.com/items',
      title: 'Create item',
      properties: [],
    };

    const template = new Template(dto);

    expect(template.contentType).toBe('application/json');
    expect(template.method).toBe('POST');
    expect(template.target).toBe('https://example.com/items');
    expect(template.title).toBe('Create item');
  });

  describe('values', () => {
    it('returns a map of property names to their values', () => {
      const template = new Template({
        properties: [
          buildProperty({ name: 'name', value: 'John' }),
          buildProperty({ name: 'age', type: PropertyType.Number, value: 30 }),
        ],
      });

      expect(template.values).toEqual({ name: 'John', age: 30 });
    });

    it('returns an empty object when there are no properties', () => {
      const template = new Template({ properties: [] });

      expect(template.values).toEqual({});
    });
  });

  describe('propertiesRecord', () => {
    it('returns a map of property names to Property instances', () => {
      const template = new Template({
        properties: [buildProperty({ name: 'name', value: 'John' })],
      });

      const record = template.propertiesRecord;

      expect(record['name']).toBeInstanceOf(Property);
      expect(record['name']!.value).toBe('John');
    });

    it('caches the computed record across repeated access', () => {
      const template = new Template({
        properties: [buildProperty({ name: 'name', value: 'John' })],
      });

      const first = template.propertiesRecord;
      const second = template.propertiesRecord;

      expect(first).toBe(second);
    });

    it('returns an empty object when there are no properties', () => {
      const template = new Template({ properties: [] });

      expect(template.propertiesRecord).toEqual({});
    });
  });

  describe('isNumberTemplate', () => {
    it('returns false for a regular Template with a string title', () => {
      const template = new Template({ properties: [], title: 'default' });

      expect(template.isNumberTemplate()).toBe(false);
    });
  });
});

describe('NumberTemplate', () => {
  it('parses a numeric title string into a number', () => {
    const template = new NumberTemplate({ properties: [], title: '3' });

    expect(template.title).toBe(3);
  });

  it('throws when the title cannot be parsed into an integer', () => {
    expect(() => new NumberTemplate({ properties: [], title: 'not-a-number' })).toThrow();
  });

  it('is recognized as a number template by isNumberTemplate()', () => {
    const template = new NumberTemplate({ properties: [], title: '0' });

    expect(template.isNumberTemplate()).toBe(true);
  });

  describe('isNumberTemplate (static)', () => {
    it('returns true for a NumberTemplate', () => {
      const template = new NumberTemplate({ properties: [], title: '1' });

      expect(NumberTemplate.isNumberTemplate(template)).toBe(true);
    });

    it('returns false for a regular Template', () => {
      const template = new Template({ properties: [], title: 'default' });

      expect(NumberTemplate.isNumberTemplate(template)).toBe(false);
    });
  });
});

describe('FormsResource', () => {
  function buildFormsResourceDto(templates: FormsResourceDto['_templates']): FormsResourceDto {
    return {
      _links: { self: [{ href: '/items/1' }] },
      _templates: templates,
    };
  }

  it('defaults _templates to an empty object when the dto has none', () => {
    const resource = new FormsResource({ _links: { self: [{ href: '/items/1' }] } });

    expect(resource._templates).toEqual({});
  });

  it('builds Template instances for every entry in _templates', () => {
    const resource = new FormsResource(buildFormsResourceDto({
      default: { properties: [buildProperty({ name: 'name', value: '' })] },
    }));

    expect(resource._templates['default']).toBeInstanceOf(Template);
  });

  describe('getTemplate', () => {
    it('returns the template with the given name', () => {
      const resource = new FormsResource(buildFormsResourceDto({
        default: { properties: [] },
      }));

      const template = resource.getTemplate('default');

      expect(template).toBeInstanceOf(Template);
    });

    it('throws when no template with the given name exists', () => {
      const resource = new FormsResource(buildFormsResourceDto({
        default: { properties: [] },
      }));

      expect(() => resource.getTemplate('missing')).toThrow(/does not have a _template with the name/);
    });
  });

  describe('getTemplateByTitle', () => {
    it('returns the template with the given title', () => {
      const resource = new FormsResource(buildFormsResourceDto({
        default: { properties: [], title: 'Create item' },
      }));

      const template = resource.getTemplateByTitle('Create item');

      expect(template).toBeInstanceOf(Template);
    });

    it('throws when no template with the given title exists', () => {
      const resource = new FormsResource(buildFormsResourceDto({
        default: { properties: [], title: 'Create item' },
      }));

      expect(() => resource.getTemplateByTitle('missing')).toThrow(/does not have a _template with the title/);
    });
  });
});
