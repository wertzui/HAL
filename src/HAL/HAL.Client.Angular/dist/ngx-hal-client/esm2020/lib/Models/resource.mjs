import { Link } from "./link";
import * as _ from 'lodash';
/**
 *  A Resource Object represents a resource.
 *  It has two reserved properties:
 *  (1)  "_links": contains links to other resources.
 *  (2)  "_embedded": contains embedded resources.
 */
export class Resource {
    constructor(dto) {
        const links = !(dto?._links) ? {} : Object.fromEntries(Object.entries(dto._links).map(([rel, links]) => [rel, Link.fromDtos(links)]));
        if (!links['self'])
            throw new Error(`The self link is missing in the given ResourceDto: ${JSON.stringify(dto)}`);
        const embedded = !(dto?._embedded) ? {} : Object.fromEntries(Object.entries(dto._embedded).map(([rel, resources]) => [rel, Resource.fromDtos(resources)]));
        const dtoWithParsedDates = Resource.parseDates(dto);
        Object.assign(this, dtoWithParsedDates);
        // We ensured that it has a self property
        this._links = links;
        this._embedded = embedded;
    }
    findLinks(rel) {
        const linksWithRel = this._links[rel];
        if (!linksWithRel)
            return [];
        return linksWithRel;
    }
    findLink(rel, name) {
        const linksWithRel = this.findLinks(rel);
        if (linksWithRel.length === 0)
            return undefined;
        if (name)
            return linksWithRel.find(link => link.name === name);
        return linksWithRel[0];
    }
    findEmbedded(rel) {
        const embeddedWithRel = this._embedded[rel];
        if (!embeddedWithRel)
            return [];
        return embeddedWithRel;
    }
    getFormLinkHrefs() {
        const allLinks = this._links;
        if (!allLinks)
            return [];
        return Object.keys(allLinks)
            .filter(key => Resource.isUrl(key));
    }
    static isUrl(possibleUrl) {
        try {
            new URL(possibleUrl);
            return true;
        }
        catch {
            return false;
        }
    }
    //public static fromDto(dto: ResourceDto): Resource;
    //public static fromDto<TResource extends Resource>(dto: ResourceDto, TResource: { new(dto: ResourceDto): TResource }): TResource;
    static fromDto(dto, TResource) {
        const links = !(dto?._links) ? {} : Object.fromEntries(Object.entries(dto._links).map(([rel, links]) => [rel, Link.fromDtos(links)]));
        const embedded = !(dto?._embedded) ? {} : Object.fromEntries(Object.entries(dto._embedded).map(([rel, embeddedResourceDtos]) => [rel, Resource.fromDtos(embeddedResourceDtos, TResource)]));
        const dtoWithParsedDates = Resource.parseDates(dto);
        const resource = Object.assign(TResource ? new TResource(dto) : new Resource(dto), dtoWithParsedDates, { _embedded: embedded, _links: links });
        return resource;
    }
    static fromDtos(dtos, TResource) {
        if (!dtos)
            return [];
        const resources = dtos
            .filter(dto => !!dto)
            .map(dto => Resource.fromDto(dto, TResource));
        return resources;
    }
    static parseDates(dto) {
        if (dto === null || dto === undefined)
            return dto;
        if (_.isString(dto)) {
            if (this._iso8601RegEx.test(dto))
                return new Date(dto);
        }
        else if (_.isArray(dto)) {
            for (let i = 0; i < dto.length; i++) {
                dto[i] = this.parseDates(dto[i]);
            }
        }
        else if (_.isPlainObject(dto)) {
            for (const [key, value] of Object.entries(dto)) {
                dto[key] = this.parseDates(value);
            }
        }
        return dto;
    }
}
Resource._iso8601RegEx = /^([+-]?\d{4}(?!\d{2}\b))((-?)((0[1-9]|1[0-2])(\3([12]\d|0[1-9]|3[01]))?|W([0-4]\d|5[0-2])(-?[1-7])?|(00[1-9]|0[1-9]\d|[12]\d{2}|3([0-5]\d|6[1-6])))([T\s]((([01]\d|2[0-3])((:?)[0-5]\d)?|24:?00)([.,]\d+(?!:))?)?(\17[0-5]\d([.,]\d+)?)?([zZ]|([+-])([01]\d|2[0-3]):?([0-5]\d)?)?)?)?$/;
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoicmVzb3VyY2UuanMiLCJzb3VyY2VSb290IjoiIiwic291cmNlcyI6WyIuLi8uLi8uLi8uLi8uLi9wcm9qZWN0cy9uZ3gtaGFsLWNsaWVudC9zcmMvbGliL01vZGVscy9yZXNvdXJjZS50cyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFBQSxPQUFPLEVBQUUsSUFBSSxFQUFXLE1BQU0sUUFBUSxDQUFDO0FBQ3ZDLE9BQU8sS0FBSyxDQUFDLE1BQU0sUUFBUSxDQUFDO0FBOEI1Qjs7Ozs7R0FLRztBQUNILE1BQU0sT0FBTyxRQUFRO0lBc0JuQixZQUFtQixHQUFpQjtRQUNsQyxNQUFNLEtBQUssR0FBRyxDQUFDLENBQUMsR0FBRyxFQUFFLE1BQU0sQ0FBQyxDQUFDLENBQUMsQ0FBQyxFQUFFLENBQUMsQ0FBQyxDQUFDLE1BQU0sQ0FBQyxXQUFXLENBQUMsTUFBTSxDQUFDLE9BQU8sQ0FBQyxHQUFHLENBQUMsTUFBTSxDQUFDLENBQUMsR0FBRyxDQUFDLENBQUMsQ0FBQyxHQUFHLEVBQUUsS0FBSyxDQUFDLEVBQUUsRUFBRSxDQUFDLENBQUMsR0FBRyxFQUFFLElBQUksQ0FBQyxRQUFRLENBQUMsS0FBSyxDQUFDLENBQUMsQ0FBQyxDQUFDLENBQUM7UUFDdEksSUFBSSxDQUFDLEtBQUssQ0FBQyxNQUFNLENBQUM7WUFDaEIsTUFBTSxJQUFJLEtBQUssQ0FBQyxzREFBc0QsSUFBSSxDQUFDLFNBQVMsQ0FBQyxHQUFHLENBQUMsRUFBRSxDQUFDLENBQUM7UUFFL0YsTUFBTSxRQUFRLEdBQUcsQ0FBQyxDQUFDLEdBQUcsRUFBRSxTQUFTLENBQUMsQ0FBQyxDQUFDLENBQUMsRUFBRSxDQUFDLENBQUMsQ0FBQyxNQUFNLENBQUMsV0FBVyxDQUFDLE1BQU0sQ0FBQyxPQUFPLENBQUMsR0FBRyxDQUFDLFNBQVMsQ0FBQyxDQUFDLEdBQUcsQ0FBQyxDQUFDLENBQUMsR0FBRyxFQUFFLFNBQVMsQ0FBQyxFQUFFLEVBQUUsQ0FBQyxDQUFDLEdBQUcsRUFBRSxRQUFRLENBQUMs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LENBQUMsR0FBRyxDQUFDLEVBQUUsQ0FBQyxHQUFHLEdBQUcsQ0FBQyxNQUFNLEVBQUUsQ0FBQyxFQUFFLEVBQUU7Z0JBQ25DLEdBQUcsQ0FBQyxDQUFDLENBQUMsR0FBRyxJQUFJLENBQUMsVUFBVSxDQUFDLEdBQUcsQ0FBQyxDQUFDLENBQUMsQ0FBQyxDQUFDO2FBQ2xDO1NBQ0Y7YUFFSSxJQUFJLENBQUMsQ0FBQyxhQUFhLENBQUMsR0FBRyxDQUFDLEVBQUU7WUFDN0IsS0FBSyxNQUFNLENBQUMsR0FBRyxFQUFFLEtBQUssQ0FBQyxJQUFJLE1BQU0sQ0FBQyxPQUFPLENBQUMsR0FBRyxDQUFDLEVBQUU7Z0JBQzlDLEdBQUcsQ0FBQyxHQUFHLENBQUMsR0FBRyxJQUFJLENBQUMsVUFBVSxDQUFDLEtBQUssQ0FBQyxDQUFDO2FBQ25DO1NBQ0Y7UUFFRCxPQUFPLEdBQUcsQ0FBQztJQUNiLENBQUM7O0FBdEljLHNCQUFhLEdBQUcsd1JBQXdSLENBQUMiLCJzb3VyY2VzQ29udGVudCI6WyJpbXBvcnQgeyBMaW5rLCBMaW5rRHRvIH0gZnJvbSBcIi4vbGlua1wiO1xyXG5pbXBvcnQgKiBhcyBfIGZyb20gJ2xvZGFzaCc7XHJcblxyXG4vKiogXHJcbiAqICBBIFJlc291cmNlIE9iamVjdCByZXByZXNlbnRzIGEgcmVzb3VyY2UuICAgXHJcbiAqICBJdCBoYXMgdHdvIHJlc2VydmVkIHByb3BlcnRpZXM6XHJcbiAqICAoMSkgIFwiX2xpbmtzXCI6IGNvbnRhaW5zIGxpbmtzIHRvIG90aGVyIHJlc291cmNlcy5cclxuICogICgyKSAgXCJfZW1iZWRkZWRcIjogY29udGFpbnMgZW1iZWRkZWQgcmVzb3VyY2VzLiBcclxuICovXHJcbmV4cG9ydCBpbnRlcmZhY2UgUmVzb3VyY2VEdG8ge1xyXG4gIC8qKiBcclxuICAgKiAgVGhlIHJlc2VydmVkIFwiX2VtYmVkZGVkXCIgcHJvcGVydHkgaXMgT1BUSU9OQUwgICBcclxuICAgKiAgSXQgaXMgYW4gb2JqZWN0IHdob3NlIHByb3BlcnR5IG5hbWVzIGFyZSBsaW5rIHJlbGF0aW9uIHR5cGVzIChhcyAgIFxyXG4gICAqICBkZWZpbmVkIGJ5IFtSRkM1OTg4XSkgYW5kIHZhbHVlcyBhcmUgZWl0aGVyIGEgUmVzb3VyY2UgT2JqZWN0IG9yIGFuICAgXHJcbiAgICogIGFycmF5IG9mIFJlc291cmNlIE9iamVjdHMuICAgRW1iZWRkZWQgUmVzb3VyY2VzIE1BWSBiZSBhIGZ1bGwsIHBhcnRpYWwsIFxyXG4gICAqICBvciBpbmNvbnNpc3RlbnQgdmVyc2lvbiBvZiAgIHRoZSByZXByZXNlbnRhdGlvbiBzZXJ2ZWQgZnJvbSB0aGUgdGFyZ2V0IFVSSS4gXHJcbiAgICovXHJcbiAgX2VtYmVkZGVkPzogeyBbbmFtZTogc3RyaW5nXTogUmVzb3VyY2VEdG9bXSB9O1xyXG4gIC8qKiBcclxuICAgKiAgVGhlIHJlc2VydmVkIFwiX2xpbmtzXCIgcHJvcGVydHkgaXMgT1BUSU9OQUwuICAgXHJcbiAgICogIEl0IGlzIGFuIG9iamVjdCB3aG9zZSBwcm9wZXJ0eSBuYW1lcyBhcmUgbGluayByZWxhdGlvbiB0eXBlcyAoYXMgICBcclxuICAgKiAgZGVmaW5lZCBieSBbUkZDNTk4OF0pIGFuZCB2YWx1ZXMgYXJlIGVpdGhlciBhIExpbmsgT2JqZWN0IG9yIGFuIGFycmF5ICAgXHJcbiAgICogIG9mIExpbmsgT2JqZWN0cy4gIFRoZSBzdWJqZWN0IHJlc291cmNlIG9mIHRoZXNlIGxpbmtzIGlzIHRoZSBSZXNvdXJjZSAgIFxyXG4gICAqICBPYmplY3Qgb2Ygd2hpY2ggdGhlIGNvbnRhaW5pbmcgXCJfbGlua3NcIiBvYmplY3QgaXMgYSBwcm9wZXJ0eS4gXHJcbiAgICovXHJcbiAgX2xpbmtzPzoge1xyXG4gICAgW25hbWU6IHN0cmluZ106IExpbmtEdG9bXSB8IG51bGwgfCB1bmRlZmluZWQ7XHJcbiAgICBzZWxmOiBMaW5rRHRvW107XHJcbiAgfTtcclxufVxyXG5cclxuLyoqIFxyXG4gKiAgQSBSZXNvdXJjZSBPYmplY3QgcmVwcmVzZW50cyBhIHJlc291cmNlLiAgIFxyXG4gKiAgSXQgaGFzIHR3byByZXNlcnZlZCBwcm9wZXJ0aWVzOlxyXG4gKiAgKDEpICBcIl9saW5rc1wiOiBjb250YWlucyBsaW5rcyB0byBvdGhlciByZXNvdXJjZXMuXHJcbiAqICAoMikgIFwiX2VtYmVkZGVkXCI6IGNvbnRhaW5zIGVtYmVkZGVkIHJlc291cmNlcy4gXHJcbiAqL1xyXG5leHBvcnQgY2xhc3MgUmVzb3VyY2Uge1xyXG4gIHByaXZhdGUgc3RhdGljIF9pc284NjAxUmVnRXggPSAvXihbKy1dP1xcZHs0fSg/IVxcZHsyfVxcYikpKCgtPykoKDBbMS05XXwxWzAtMl0pKFxcMyhbMTJdXFxkfDBbMS05XXwzWzAxXSkpP3xXKFswLTRdXFxkfDVbMC0yXSkoLT9bMS03XSk/fCgwMFsxLTldfDBbMS05XVxcZHxbMTJdXFxkezJ9fDMoWzAtNV1cXGR8NlsxLTZdKSkpKFtUXFxzXSgoKFswMV1cXGR8MlswLTNdKSgoOj8pWzAtNV1cXGQpP3wyNDo/MDApKFsuLF1cXGQrKD8hOikpPyk/KFxcMTdbMC01XVxcZChbLixdXFxkKyk/KT8oW3paXXwoWystXSkoWzAxXVxcZHwyWzAtM10pOj8oWzAtNV1cXGQpPyk/KT8pPyQvO1xyXG4gIC8qKiBcclxuICAgKiAgVGhlIHJlc2VydmVkIFwiX2VtYmVkZGVkXCIgcHJvcGVydHkgaXMgT1BUSU9OQUwgICBcclxuICAgKiAgSXQgaXMgYW4gb2JqZWN0IHdob3NlIHByb3BlcnR5IG5hbWVzIGFyZSBsaW5rIHJlbGF0aW9uIHR5cGVzIChhcyAgIFxyXG4gICAqICBkZWZpbmVkIGJ5IFtSRkM1OTg4XSkgYW5kIHZhbHVlcyBhcmUgZWl0aGVyIGEgUmVzb3VyY2UgT2JqZWN0IG9yIGFuICAgXHJcbiAgICogIGFycmF5IG9mIFJlc291cmNlIE9iamVjdHMuICAgRW1iZWRkZWQgUmVzb3VyY2VzIE1BWSBiZSBhIGZ1bGwsIHBhcnRpYWwsIFxyXG4gICAqICBvciBpbmNvbnNpc3RlbnQgdmVyc2lvbiBvZiAgIHRoZSByZXByZXNlbnRhdGlvbiBzZXJ2ZWQgZnJvbSB0aGUgdGFyZ2V0IFVSSS4gXHJcbiAgICovXHJcbiAgcHVibGljIF9lbWJlZGRlZDogeyBbbmFtZTogc3RyaW5nXTogUmVzb3VyY2VbXSB9O1xyXG4gIC8qKiBcclxuICAgKiAgVGhlIHJlc2VydmVkIFwiX2xpbmtzXCIgcHJvcGVydHkgaXMgT1BUSU9OQUwuICAgXHJcbiAgICogIEl0IGlzIGFuIG9iamVjdCB3aG9zZSBwcm9wZXJ0eSBuYW1lcyBhcmUgbGluayByZWxhdGlvbiB0eXBlcyAoYXMgICBcclxuICAgKiAgZGVmaW5lZCBieSBbUkZDNTk4OF0pIGFuZCB2YWx1ZXMgYXJlIGVpdGhlciBhIExpbmsgT2JqZWN0IG9yIGFuIGFycmF5ICAgXHJcbiAgICogIG9mIExpbmsgT2JqZWN0cy4gIFRoZSBzdWJqZWN0IHJlc291cmNlIG9mIHRoZXNlIGxpbmtzIGlzIHRoZSBSZXNvdXJjZSAgIFxyXG4gICAqICBPYmplY3Qgb2Ygd2hpY2ggdGhlIGNvbnRhaW5pbmcgXCJfbGlua3NcIiBvYmplY3QgaXMgYSBwcm9wZXJ0eS4gXHJcbiAgICovXHJcbiAgcHVibGljIF9saW5rczoge1xyXG4gICAgW25hbWU6IHN0cmluZ106IExpbmtbXSB8IHVuZGVmaW5lZDtcclxuICAgIHNlbGY6IExpbmtbXTtcclxuICB9O1xyXG5cclxuICBwdWJsaWMgY29uc3RydWN0b3IoZHRvPzogUmVzb3VyY2VEdG8pIHtcclxuICAgIGNvbnN0IGxpbmtzID0gIShkdG8/Ll9saW5rcykgPyB7fSA6IE9iamVjdC5mcm9tRW50cmllcyhPYmplY3QuZW50cmllcyhkdG8uX2xpbmtzKS5tYXAoKFtyZWwsIGxpbmtzXSkgPT4gW3JlbCwgTGluay5mcm9tRHRvcyhsaW5rcyldKSk7XHJcbiAgICBpZiAoIWxpbmtzWydzZWxmJ10pXHJcbiAgICAgIHRocm93IG5ldyBFcnJvcihgVGhlIHNlbGYgbGluayBpcyBtaXNzaW5nIGluIHRoZSBnaXZlbiBSZXNvdXJjZUR0bzogJHtKU09OLnN0cmluZ2lmeShkdG8pfWApO1xyXG5cclxuICAgIGNvbnN0IGVtYmVkZGVkID0gIShkdG8/Ll9lbWJlZGRlZCkgPyB7fSA6IE9iamVjdC5mcm9tRW50cmllcyhPYmplY3QuZW50cmllcyhkdG8uX2VtYmVkZGVkKS5tYXAoKFtyZWwsIHJlc291cmNlc10pID0+IFtyZWwsIFJlc291cmNlLmZyb21EdG9zKHJlc291cmNlcyldKSk7XHJcbiAgICBjb25zdCBkdG9XaXRoUGFyc2VkRGF0ZXMgPSBSZXNvdXJjZS5wYXJzZURhdGVzKGR0byk7XHJcblxyXG4gICAgT2JqZWN0LmFzc2lnbih0aGlzLCBkdG9XaXRoUGFyc2VkRGF0ZXMpO1xyXG5cclxuICAgIC8vIFdlIGVuc3VyZWQgdGhhdCBpdCBoYXMgYSBzZWxmIHByb3BlcnR5XHJcbiAgICB0aGlzLl9saW5rcyA9IGxpbmtzIGFzIHtcclxuICAgICAgW25hbWU6IHN0cmluZ106IExpbmtbXSB8IHVuZGVmaW5lZDtcclxuICAgICAgc2VsZjogTGlua1tdO1xyXG4gICAgfTtcclxuICAgIHRoaXMuX2VtYmVkZGVkID0gZW1iZWRkZWQ7XHJcbiAgfVxyXG5cclxuICBwdWJsaWMgZmluZExpbmtzKHJlbDogc3RyaW5nKTogTGlua1tdIHtcclxuICAgIGNvbnN0IGxpbmtzV2l0aFJlbCA9IHRoaXMuX2xpbmtzW3JlbF07XHJcblxyXG4gICAgaWYgKCFsaW5rc1dpdGhSZWwpXHJcbiAgICAgIHJldHVybiBbXTtcclxuXHJcbiAgICByZXR1cm4gbGlua3NXaXRoUmVsO1xyXG4gIH1cclxuXHJcbiAgcHVibGljIGZpbmRMaW5rKHJlbDogc3RyaW5nLCBuYW1lPzogc3RyaW5nKTogTGluayB8IHVuZGVmaW5lZCB7XHJcbiAgICBjb25zdCBsaW5rc1dpdGhSZWwgPSB0aGlzLmZpbmRMaW5rcyhyZWwpO1xyXG5cclxuICAgIGlmIChsaW5rc1dpdGhSZWwubGVuZ3RoID09PSAwKVxyXG4gICAgICByZXR1cm4gdW5kZWZpbmVkO1xyXG5cclxuICAgIGlmIChuYW1lKVxyXG4gICAgICByZXR1cm4gbGlua3NXaXRoUmVsLmZpbmQobGluayA9PiBsaW5rLm5hbWUgPT09IG5hbWUpO1xyXG5cclxuICAgIHJldHVybiBsaW5rc1dpdGhSZWxbMF07XHJcbiAgfVxyXG5cclxuICBwdWJsaWMgZmluZEVtYmVkZGVkKHJlbDogc3RyaW5nKTogUmVzb3VyY2VbXSB7XHJcbiAgICBjb25zdCBlbWJlZGRlZFdpdGhSZWwgPSB0aGlzLl9lbWJlZGRlZFtyZWxdO1xyXG5cclxuICAgIGlmICghZW1iZWRkZWRXaXRoUmVsKVxyXG4gICAgICByZXR1cm4gW107XHJcblxyXG4gICAgcmV0dXJuIGVtYmVkZGVkV2l0aFJlbDtcclxuICB9XHJcblxyXG4gIHB1YmxpYyBnZXRGb3JtTGlua0hyZWZzKCk6IHN0cmluZ1tdIHtcclxuICAgIGNvbnN0IGFsbExpbmtzID0gdGhpcy5fbGlua3M7XHJcblxyXG4gICAgaWYgKCFhbGxMaW5rcylcclxuICAgICAgcmV0dXJuIFtdO1xyXG5cclxuICAgIHJldHVybiBPYmplY3Qua2V5cyhhbGxMaW5rcylcclxuICAgICAgLmZpbHRlcihrZXkgPT4gUmVzb3VyY2UuaXNVcmwoa2V5KSk7XHJcbiAgfVxyXG5cclxuICBwcml2YXRlIHN0YXRpYyBpc1VybChwb3NzaWJsZVVybDogc3RyaW5nKTogYm9vbGVhbiB7XHJcbiAgICB0cnkge1xyXG4gICAgICBuZXcgVVJMKHBvc3NpYmxlVXJsKTtcclxuICAgICAgcmV0dXJuIHRydWU7XHJcbiAgICB9XHJcbiAgICBjYXRjaCB7XHJcbiAgICAgIHJldHVybiBmYWxzZTtcclxuICAgIH1cclxuICB9XHJcblxyXG4gIC8vcHVibGljIHN0YXRpYyBmcm9tRHRvKGR0bzogUmVzb3VyY2VEdG8pOiBSZXNvdXJjZTtcclxuICAvL3B1YmxpYyBzdGF0aWMgZnJvbUR0bzxUUmVzb3VyY2UgZXh0ZW5kcyBSZXNvdXJjZT4oZHRvOiBSZXNvdXJjZUR0bywgVFJlc291cmNlOiB7IG5ldyhkdG86IFJlc291cmNlRHRvKTogVFJlc291cmNlIH0pOiBUUmVzb3VyY2U7XHJcbiAgcHJpdmF0ZSBzdGF0aWMgZnJvbUR0bzxUUmVzb3VyY2UgZXh0ZW5kcyBSZXNvdXJjZT4oZHRvOiBSZXNvdXJjZUR0bywgVFJlc291cmNlPzogeyBuZXcoZHRvOiBSZXNvdXJjZUR0byk6IFRSZXNvdXJjZSB9KTogVFJlc291cmNlIHwgUmVzb3VyY2Uge1xyXG4gICAgY29uc3QgbGlua3MgPSAhKGR0bz8uX2xpbmtzKSA/IHt9IDogT2JqZWN0LmZyb21FbnRyaWVzKE9iamVjdC5lbnRyaWVzKGR0by5fbGlua3MpLm1hcCgoW3JlbCwgbGlua3NdKSA9PiBbcmVsLCBMaW5rLmZyb21EdG9zKGxpbmtzKV0pKTtcclxuICAgIGNvbnN0IGVtYmVkZGVkID0gIShkdG8/Ll9lbWJlZGRlZCkgPyB7fSA6IE9iamVjdC5mcm9tRW50cmllcyhPYmplY3QuZW50cmllcyhkdG8uX2VtYmVkZGVkKS5tYXAoKFtyZWwsIGVtYmVkZGVkUmVzb3VyY2VEdG9zXSkgPT4gW3JlbCwgUmVzb3VyY2UuZnJvbUR0b3MoZW1iZWRkZWRSZXNvdXJjZUR0b3MsIFRSZXNvdXJjZSldKSk7XHJcbiAgICBjb25zdCBkdG9XaXRoUGFyc2VkRGF0ZXMgPSBSZXNvdXJjZS5wYXJzZURhdGVzKGR0byk7XHJcblxyXG4gICAgY29uc3QgcmVzb3VyY2UgPSBPYmplY3QuYXNzaWduKFRSZXNvdXJjZSA/IG5ldyBUUmVzb3VyY2UoZHRvKSA6IG5ldyBSZXNvdXJjZShkdG8pLCBkdG9XaXRoUGFyc2VkRGF0ZXMsIHsgX2VtYmVkZGVkOiBlbWJlZGRlZCwgX2xpbmtzOiBsaW5rcyB9KTtcclxuXHJcbiAgICByZXR1cm4gcmVzb3VyY2U7XHJcbiAgfVxyXG5cclxuICBwcml2YXRlIHN0YXRpYyBmcm9tRHRvczxUUmVzb3VyY2UgZXh0ZW5kcyBSZXNvdXJjZT4oZHRvczogUmVzb3VyY2VEdG9bXSB8IG51bGwgfCB1bmRlZmluZWQsIFRSZXNvdXJjZT86IHsgbmV3KGR0bzogUmVzb3VyY2VEdG8pOiBUUmVzb3VyY2UgfSk6IChUUmVzb3VyY2UgfCBSZXNvdXJjZSlbXSB7XHJcbiAgICBpZiAoIWR0b3MpXHJcbiAgICAgIHJldHVybiBbXTtcclxuXHJcbiAgICBjb25zdCByZXNvdXJjZXMgPSBkdG9zXHJcbiAgICAgIC5maWx0ZXIoZHRvID0+ICEhZHRvKVxyXG4gICAgICAubWFwKGR0byA9PiBSZXNvdXJjZS5mcm9tRHRvKGR0bywgVFJlc291cmNlKSk7XHJcblxyXG4gICAgcmV0dXJuIHJlc291cmNlcztcclxuICB9XHJcblxyXG4gIHByaXZhdGUgc3RhdGljIHBhcnNlRGF0ZXMoZHRvOiBhbnkpOiBhbnkge1xyXG4gICAgaWYgKGR0byA9PT0gbnVsbCB8fCBkdG8gPT09IHVuZGVmaW5lZClcclxuICAgICAgcmV0dXJuIGR0bztcclxuXHJcbiAgICBpZiAoXy5pc1N0cmluZyhkdG8pKSB7XHJcbiAgICAgIGlmICh0aGlzLl9pc284NjAxUmVnRXgudGVzdChkdG8pKVxyXG4gICAgICAgIHJldHVybiBuZXcgRGF0ZShkdG8pO1xyXG4gICAgfVxyXG5cclxuICAgIGVsc2UgaWYgKF8uaXNBcnJheShkdG8pKSB7XHJcbiAgICAgIGZvciAobGV0IGkgPSAwOyBpIDwgZHRvLmxlbmd0aDsgaSsrKSB7XHJcbiAgICAgICAgZHRvW2ldID0gdGhpcy5wYXJzZURhdGVzKGR0b1tpXSk7XHJcbiAgICAgIH1cclxuICAgIH1cclxuXHJcbiAgICBlbHNlIGlmIChfLmlzUGxhaW5PYmplY3QoZHRvKSkge1xyXG4gICAgICBmb3IgKGNvbnN0IFtrZXksIHZhbHVlXSBvZiBPYmplY3QuZW50cmllcyhkdG8pKSB7XHJcbiAgICAgICAgZHRvW2tleV0gPSB0aGlzLnBhcnNlRGF0ZXModmFsdWUpO1xyXG4gICAgICB9XHJcbiAgICB9XHJcblxyXG4gICAgcmV0dXJuIGR0bztcclxuICB9XHJcbn1cclxuIl19