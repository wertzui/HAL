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
    return resource && (resource instanceof ProblemDetails || (resource instanceof Resource && ProblemDetails.hasValidHttpStatus(resource)));
  }

  public static isProblemDetailsDto(dto: unknown): dto is ProblemDetailsDto {
    return typeof dto === "object" && dto !== null && ProblemDetails.hasValidHttpStatus(dto);
  }

  public static hasValidHttpStatus(dto: object): dto is { status: number } {
    return 'status' in dto && typeof dto.status === "number" && Number.isInteger(dto.status) && dto.status >= 100 && dto.status < 600;
  }
}
