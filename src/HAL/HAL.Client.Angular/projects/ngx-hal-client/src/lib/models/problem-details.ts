import * as _ from "lodash";
import { Resource, ResourceDto, } from "./resource"
import { ResourceOfDto } from "./resourceOf";
/*
 * ProblemDetails is what an ASP.net Core backend returns in case of an error.
 * */
export interface ProblemDetailsDto {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  [key: string]: unknown;
}

export class ProblemDetails extends Resource implements ResourceOfDto<ProblemDetailsDto> {
  public declare type?: string;
  public declare title?: string;
  public declare status?: number;
  public declare detail?: string;
  public declare instance?: string;
  [key: string]: unknown;

  public constructor(dto: ProblemDetailsDto & ResourceDto) {
    super(dto);
  }

  public static isProblemDetails(resource: unknown): resource is ProblemDetails {
    return resource instanceof ProblemDetails;
  }

  public static containsProblemDetailsInformation(resource: unknown) {
    return resource && (resource instanceof ProblemDetails || (resource instanceof Resource && 'status' in resource && _.isNumber(resource['status']) && resource['status'] >= 100 && resource['status'] < 600));
  }

  public static isProblemDetailsDto(dto: unknown): dto is ProblemDetailsDto {
    return _.isObject(dto) && 'status' in dto && _.isNumber(dto['status']) && dto['status'] >= 100 && dto['status'] < 600;
  }
}
