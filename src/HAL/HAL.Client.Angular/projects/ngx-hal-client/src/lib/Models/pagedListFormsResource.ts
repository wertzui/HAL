import { Template, TemplateDtos, Templates } from "./formsResource"
import { Link, LinkDto } from "./link";
import { Page } from "./page";
import { PagedListResourceDto, PagedListResource } from "./pagedListResource"
import { ResourceDto } from "./resource";

export interface PagedListFormsResourceDto<TListDto extends ResourceDto> extends PagedListResourceDto<TListDto> {
  _links?: {
    [name: string]: LinkDto[] | null | undefined;
    self: LinkDto[];
    first?: LinkDto[];
    prev?: LinkDto[];
    next?: LinkDto[];
    last?: LinkDto[];
  };
  _templates?: TemplateDtos;
}

export class PagedListFormsResource<TListDto extends ResourceDto> extends PagedListResource<TListDto> implements Page {
  _links!: {
    [name: string]: Link[] | undefined;
    self: Link[];
    first?: Link[];
    prev?: Link[];
    next?: Link[];
    last?: Link[];
  };

  currentPage?: number;
  totalPages?: number;
  public _templates!: Templates;

  public constructor(dto?: PagedListFormsResourceDto<TListDto>) {
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