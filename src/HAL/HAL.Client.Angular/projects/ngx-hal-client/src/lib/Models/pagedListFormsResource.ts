import { FormsResourceDto, Template, TemplateDtos, Templates } from "./formsResource"
import { Link, LinkDto } from "./link";
import { Page } from "./page";
import { PagedListResourceDto, PagedListResource } from "./pagedListResource"
import { ResourceDto } from "./resource";

/**
 * A PagedListFormsResourceDto is a @see PagedListResourceDto which is also a @see FormsResourceDto.
 * It is normally used to give search end edit templates in conjunction with a paged list of resources.
 * @template TListDto The type of the resource in the list.
 */
export interface PagedListFormsResourceDto<TListDto> extends PagedListResourceDto<TListDto>, Omit<Omit<FormsResourceDto, "_links">, "_embedded"> {
}

/**
 * A PagedListFormsResourceDto is a @see PagedListResource which is also a @see FormsResource.
 * It is normally used to give search end edit templates in conjunction with a paged list of resources.
 * @template TListDto The type of the resource in the list.
 */
export class PagedListFormsResource<TListDto> extends PagedListResource<TListDto> implements Page {
  public _templates!: Templates;

  public constructor(dto: PagedListFormsResourceDto<TListDto>) {
    super(dto);

    this._templates = (!(dto?._templates) ? {} : Object.fromEntries(Object.entries(dto._templates).map(([rel, templateDto]) => [rel, new Template(templateDto)]))) as Templates;
  }

  public getTemplate(name: string) {
    const templateNames = Object.getOwnPropertyNames(this._templates);
    if (!templateNames.includes(name))
      throw new Error(`The form ${this} does not have a _template with the name '${name}'. It only has ${templateNames}.`);

    const template = this._templates[name];

    return template;
  }
}