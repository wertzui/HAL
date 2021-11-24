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
    readOnly?: boolean;
    regex?: string;
    required?: boolean;
    rows?: number;
    step?: number;
    templated?: boolean;
    _templates?: TemplateDtos;
    type?: PropertyType;
    value?: unknown;
}
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
export declare enum PropertyType {
    Hidden = "hidden",
    Text = "text",
    Textarea = "textarea",
    Search = "search",
    Tel = "tel",
    Url = "url",
    Email = "email",
    Password = "password",
    Date = "date",
    Month = "month",
    Week = "week",
    Time = "time",
    DatetimeLocal = "datetime-local",
    Number = "number",
    Range = "range",
    Color = "color",
    Bool = "bool",
    DatetimeOffset = "datetime-offset",
    Duration = "duration",
    Image = "image",
    File = "file",
    Collection = "collection",
    Object = "object"
}
export interface Templates {
    [name: string]: Template;
}
export declare class Options {
    inline: OptionsItemDto[] | unknown[];
    link?: OptionsLinkDto;
    maxItems?: number;
    minItems?: number;
    promptField?: string;
    selectedValues?: unknown[];
    valueField?: string;
    constructor(dto?: OptionsDto);
}
export declare class Property {
    cols?: number;
    max?: number;
    maxLength?: number;
    min?: number;
    minLength?: number;
    name?: string;
    options?: Options;
    placeholder?: string;
    prompt?: string;
    readOnly?: boolean;
    regex?: string;
    required?: boolean;
    rows?: number;
    step?: number;
    templated?: boolean;
    _templates: Templates;
    type?: PropertyType;
    value?: unknown;
    constructor(dto?: PropertyDto);
}
export declare class Template {
    contentType?: string;
    method?: string;
    properties: Property[];
    target?: string;
    title?: string;
    constructor(dto?: TemplateDto);
    get values(): {
        [name: string]: unknown;
    };
}
export declare class FormsResource extends Resource {
    _templates: Templates;
    constructor(dto?: FormsResourceDto);
    getTemplate(name: string): Template;
}
