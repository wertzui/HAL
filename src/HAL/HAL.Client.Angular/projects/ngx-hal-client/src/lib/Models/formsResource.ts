import { Resource, ResourceDto } from './resource';

export interface FormsResourceDto extends ResourceDto {
  _templates?: TemplateDtos;
}

export interface TemplateDto {
  contentType?: string;
  method?: string;
  properties?: PropertyDto[];
  target?: string;
  title?: string;
}

export interface TemplateDtos {
  [name: string]: TemplateDto;
}

export interface PropertyDto {
  cols?: number;
  max?: number;
  maxLength?: number;
  min?: number;
  minLength?: number;
  name?: string;
  options?: OptionsDto;
  placeholder?: string;
  prompt?: string;
  promptDisplay?: PropertyPromptDisplayType;
  readOnly?: boolean;
  regex?: string;
  required?: boolean;
  rows?: number;
  step?: number;
  templated?: boolean;
  _templates?: TemplateDtos;
  type?: PropertyType;
  value?: unknown;
};

export interface OptionsDto {
  inline?: OptionsItemDto[] | unknown[];
  link?: OptionsLinkDto;
  maxItems?: number;
  minItems?: number;
  promptField?: string;
  selectedValues?: unknown[];
  valueField?: string;
}

export interface OptionsItemDto {
  prompt: string;
  value: unknown;
}

export interface OptionsLinkDto {
  href: string;
  templated?: boolean;
  type?: string;
}

export enum PropertyPromptDisplayType {
  Visible = 'visible',
  Hidden = 'hidden',
  Collapsed = 'collapsed'
}

export enum PropertyType {
  Hidden = 'hidden',
  Text = 'text',
  Textarea = 'textarea',
  Search = 'search',
  Tel = 'tel',
  Url = 'url',
  Email = 'email',
  Password = 'password',
  Date = 'date',
  Month = 'month',
  Week = 'week',
  Time = 'time',
  DatetimeLocal = 'datetime-local',
  Number = 'number',
  Range = 'range',
  Color = 'color',
  Bool = 'bool',
  DatetimeOffset = 'datetime-offset',
  Duration = 'duration',
  Image = 'image',
  File = 'file',
  Collection = 'collection',
  Object = 'object'
}

export interface Templates {
  [name: string]: Template;
}

export class Options {
  inline: OptionsItemDto[] | unknown[] = [];
  link?: OptionsLinkDto;
  maxItems?: number;
  minItems?: number;
  promptField?: string;
  selectedValues?: unknown[];
  valueField?: string;

  constructor(dto?: OptionsDto) {
    Object.assign(this, dto);

    if (!this.inline)
      this.inline = [];
  }
}

export class Property {
  cols?: number;
  max?: number;
  maxLength?: number;
  min?: number;
  minLength?: number;
  name?: string;
  options?: Options;
  placeholder?: string;
  prompt?: string;
  promptDisplay?: PropertyPromptDisplayType;
  readOnly?: boolean;
  regex?: string;
  required?: boolean;
  rows?: number;
  step?: number;
  templated?: boolean;
  _templates: Templates;
  type?: PropertyType;
  value?: unknown;

  public constructor(dto?: PropertyDto) {
    Object.assign(this, dto);

    this._templates = !(dto?._templates) ? {} : Object.fromEntries(Object.entries(dto._templates).map(([rel, templateDto]) => [rel, new Template(templateDto)]));

    if (this.options)
      this.options = new Options(dto?.options);
  }
};

export class Template {
  contentType?: string;
  method?: string;
  properties: Property[];
  target?: string;
  title?: string;

  constructor(dto?: TemplateDto) {
    Object.assign(this, dto);

    this.properties = !(dto?.properties) ? [] : dto.properties.map(propertyDto => new Property(propertyDto));
  }

  public get values(): { [name: string]: unknown } {
    return !this.properties ? {} : Object.fromEntries(this.properties.map(property => [property.name, property.value]));
  }

}

export class NumberTemplate implements Omit<Template, "title">{
  contentType?: string;
  method?: string;
  properties: Property[];
  target?: string;
  title: number;

  constructor(dto?: TemplateDto) {
    Object.assign(this, dto);

    if(!Number.isInteger(dto?.title))
      throw new Error(`Expected ${dto?.title} to be an integer.`);
      
    this.title = Number.parseInt(dto!.title!);

    this.properties = !(dto?.properties) ? [] : dto.properties.map(propertyDto => new Property(propertyDto));
  }

  public get values(): { [name: string]: unknown } {
    return !this.properties ? {} : Object.fromEntries(this.properties.map(property => [property.name, property.value]));
  }
}

export class FormsResource extends Resource {
  public _templates!: Templates;

  public constructor(dto?: FormsResourceDto) {
    super(dto);

    this._templates = !(dto?._templates) ? {} : Object.fromEntries(Object.entries(dto._templates).map(([rel, templateDto]) => [rel, new Template(templateDto)]));
  }

  public getTemplate(name: string) {
    const templateNames = Object.getOwnPropertyNames(this._templates);
    if (!templateNames.includes(name))
      throw new Error(`The form ${this} does not have a _template with the name '${name}'. It only has ${templateNames}.`);

    const template = this._templates[name];

    return template;
  }
}