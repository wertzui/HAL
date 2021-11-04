import { __awaiter } from "tslib";
import { HttpErrorResponse, HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Resource } from './Models/resource';
import * as i0 from "@angular/core";
import * as i1 from "@angular/common/http";
export class HalClient {
    constructor(_httpClient) {
        this._httpClient = _httpClient;
    }
    get(uri, TResource, TError, headers) {
        return __awaiter(this, void 0, void 0, function* () {
            const options = HalClient.createOptions(headers);
            let dtoResponse;
            try {
                dtoResponse = yield this._httpClient.get(uri, options).toPromise();
            }
            catch (e) {
                if (e instanceof HttpErrorResponse)
                    dtoResponse = HalClient.convertErrorResponse(e);
                else
                    throw new Error(`GET ${uri} - options: ${options} failed with error ${e}`);
            }
            if (!dtoResponse)
                throw new Error(`GET ${uri} - options: ${options} did not return a response.`);
            const resourceResponse = HalClient.convertResponse(dtoResponse.ok ? TResource : TError, dtoResponse);
            return resourceResponse;
        });
    }
    post(uri, body, TResource, TError, headers) {
        return __awaiter(this, void 0, void 0, function* () {
            const options = HalClient.createOptions(headers);
            let dtoResponse;
            try {
                dtoResponse = yield this._httpClient.post(uri, body, options).toPromise();
            }
            catch (e) {
                if (e instanceof HttpErrorResponse)
                    dtoResponse = HalClient.convertErrorResponse(e);
                else
                    throw new Error(`POST ${uri} - options: ${options} - body: ${body} failed with error ${e}`);
            }
            if (!dtoResponse)
                throw new Error(`POST ${uri} - options: ${options} - body: ${body} did not return a response.`);
            const resourceResponse = HalClient.convertResponse(dtoResponse.ok ? TResource : TError, dtoResponse);
            return resourceResponse;
        });
    }
    put(uri, body, TResource, TError, headers) {
        return __awaiter(this, void 0, void 0, function* () {
            const options = HalClient.createOptions(headers);
            let dtoResponse;
            try {
                dtoResponse = yield this._httpClient.put(uri, body, options).toPromise();
            }
            catch (e) {
                if (e instanceof HttpErrorResponse)
                    dtoResponse = HalClient.convertErrorResponse(e);
                else
                    throw new Error(`PUT ${uri} - options: ${options} - body: ${body} failed with error ${e}`);
            }
            if (!dtoResponse)
                throw new Error(`PUT ${uri} - options: ${options} - body: ${body} did not return a response.`);
            const resourceResponse = HalClient.convertResponse(dtoResponse.ok ? TResource : TError, dtoResponse);
            return resourceResponse;
        });
    }
    delete(uri, TError, headers) {
        return __awaiter(this, void 0, void 0, function* () {
            const options = HalClient.createOptions(headers);
            let response;
            try {
                response = yield this._httpClient.delete(uri, options).toPromise();
            }
            catch (e) {
                if (e instanceof HttpErrorResponse)
                    response = HalClient.convertErrorResponse(e);
                else
                    throw new Error(`DELETE ${uri} - options: ${options} failed with error ${e}`);
            }
            if (!response)
                throw new Error(`DELETE ${uri} - options: ${options} did not return a response.`);
            if (!response.ok) {
                const errorResponse = HalClient.convertResponse(TError, response);
                return errorResponse;
            }
            return response;
        });
    }
    static createOptions(headers) {
        headers === null || headers === void 0 ? void 0 : headers.append('Accept', 'application/hal+json');
        return {
            headers: headers,
            responseType: 'json',
            observe: 'response'
        };
    }
    static convertResponse(TResource, response) {
        const resource = Resource.fromDto(response.body || new TResource(), TResource);
        const resourceResponse = response.clone({ body: resource });
        return resourceResponse;
    }
    static convertErrorResponse(e) {
        const dtoResponse = new HttpResponse({ body: e.error, headers: e.headers, status: e.status, statusText: e.statusText, url: e.url ? e.url : undefined });
        return dtoResponse;
    }
}
HalClient.ɵfac = i0.ɵɵngDeclareFactory({ minVersion: "12.0.0", version: "12.2.11", ngImport: i0, type: HalClient, deps: [{ token: i1.HttpClient }], target: i0.ɵɵFactoryTarget.Injectable });
HalClient.ɵprov = i0.ɵɵngDeclareInjectable({ minVersion: "12.0.0", version: "12.2.11", ngImport: i0, type: HalClient, providedIn: 'root' });
i0.ɵɵngDeclareClassMetadata({ minVersion: "12.0.0", version: "12.2.11", ngImport: i0, type: HalClient, decorators: [{
            type: Injectable,
            args: [{
                    providedIn: 'root'
                }]
        }], ctorParameters: function () { return [{ type: i1.HttpClient }]; } });
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoiaGFsLWNsaWVudC5qcyIsInNvdXJjZVJvb3QiOiIiLCJzb3VyY2VzIjpbIi4uLy4uLy4uLy4uL3Byb2plY3RzL25neC1oYWwtY2xpZW50L3NyYy9saWIvaGFsLWNsaWVudC50cyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiO0FBQUEsT0FBTyxFQUFjLGlCQUFpQixFQUFlLFlBQVksRUFBRSxNQUFNLHNCQUFzQixDQUFDO0FBQ2hHLE9BQU8sRUFBRSxVQUFVLEVBQUUsTUFBTSxlQUFlLENBQUM7QUFDM0MsT0FBTyxFQUFFLFFBQVEsRUFBZSxNQUFNLG1CQUFtQixDQUFDOzs7QUFLMUQsTUFBTSxPQUFPLFNBQVM7SUFFcEIsWUFBb0IsV0FBdUI7UUFBdkIsZ0JBQVcsR0FBWCxXQUFXLENBQVk7SUFBSSxDQUFDO0lBRW5DLEdBQUcsQ0FBc0QsR0FBVyxFQUFFLFNBQStCLEVBQUUsTUFBeUIsRUFBRSxPQUFxQjs7WUFDbEssTUFBTSxPQUFPLEdBQUcsU0FBUyxDQUFDLGFBQWEsQ0FBQyxPQUFPLENBQUMsQ0FBQztZQUNqRCxJQUFJLFdBQWtELENBQUM7WUFDdkQsSUFBSTtnQkFDRixXQUFXLEdBQUcsTUFBTSxJQUFJLENBQUMsV0FBVyxDQUFDLEdBQUcsQ0FBYyxHQUFHLEVBQUUsT0FBTyxDQUFDLENBQUMsU0FBUyxFQUFFLENBQUM7YUFDakY7WUFDRCxPQUFPLENBQUMsRUFBRTtnQkFDUixJQUFJLENBQUMsWUFBWSxpQkFBaUI7b0JBQ2hDLFdBQVcsR0FBRyxTQUFTLENBQUMsb0JBQW9CLENBQUMsQ0FBQyxDQUFDLENBQUM7O29CQUVoRCxNQUFNLElBQUksS0FBSyxDQUFDLE9BQU8sR0FBRyxlQUFlLE9BQU8sc0JBQXNCLENBQUMsRUFBRSxDQUFDLENBQUM7YUFDOUU7WUFDRCxJQUFJLENBQUMsV0FBVztnQkFDZCxNQUFNLElBQUksS0FBSyxDQUFDLE9BQU8sR0FBRyxlQUFlLE9BQU8sNkJBQTZCLENBQUMsQ0FBQztZQUNqRixNQUFNLGdCQUFnQixHQUFHLFNBQVMs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QVcsR0FBRyxJQUFJLFlBQVksQ0FBQyxFQUFFLElBQUksRUFBRSxDQUFDLENBQUMsS0FBSyxFQUFFLE9BQU8sRUFBRSxDQUFDLENBQUMsT0FBTyxFQUFFLE1BQU0sRUFBRSxDQUFDLENBQUMsTUFBTSxFQUFFLFVBQVUsRUFBRSxDQUFDLENBQUMsVUFBVSxFQUFFLEdBQUcsRUFBRSxDQUFDLENBQUMsR0FBRyxDQUFDLENBQUMsQ0FBQyxDQUFDLENBQUMsR0FBRyxDQUFDLENBQUMsQ0FBQyxTQUFTLEVBQUUsQ0FBQyxDQUFDO1FBQ3hKLE9BQU8sV0FBVyxDQUFDO0lBQ3JCLENBQUM7O3VHQWxHVSxTQUFTOzJHQUFULFNBQVMsY0FGUixNQUFNOzRGQUVQLFNBQVM7a0JBSHJCLFVBQVU7bUJBQUM7b0JBQ1YsVUFBVSxFQUFFLE1BQU07aUJBQ25CIiwic291cmNlc0NvbnRlbnQiOlsiaW1wb3J0IHsgSHR0cENsaWVudCwgSHR0cEVycm9yUmVzcG9uc2UsIEh0dHBIZWFkZXJzLCBIdHRwUmVzcG9uc2UgfSBmcm9tICdAYW5ndWxhci9jb21tb24vaHR0cCc7XHJcbmltcG9ydCB7IEluamVjdGFibGUgfSBmcm9tICdAYW5ndWxhci9jb3JlJztcclxuaW1wb3J0IHsgUmVzb3VyY2UsIFJlc291cmNlRHRvIH0gZnJvbSAnLi9Nb2RlbHMvcmVzb3VyY2UnO1xyXG5cclxuQEluamVjdGFibGUoe1xyXG4gIHByb3ZpZGVkSW46ICdyb290J1xyXG59KVxyXG5leHBvcnQgY2xhc3MgSGFsQ2xpZW50IHtcclxuXHJcbiAgY29uc3RydWN0b3IocHJpdmF0ZSBfaHR0cENsaWVudDogSHR0cENsaWVudCkgeyB9XHJcblxyXG4gIHB1YmxpYyBhc3luYyBnZXQ8VFJlc291cmNlIGV4dGVuZHMgUmVzb3VyY2UsIFRFcnJvciBleHRlbmRzIFJlc291cmNlPih1cmk6IHN0cmluZywgVFJlc291cmNlOiB7IG5ldygpOiBUUmVzb3VyY2UgfSwgVEVycm9yOiB7IG5ldygpOiBURXJyb3IgfSwgaGVhZGVycz86IEh0dHBIZWFkZXJzKTogUHJvbWlzZTxIdHRwUmVzcG9uc2U8VFJlc291cmNlIHwgVEVycm9yPj4ge1xyXG4gICAgY29uc3Qgb3B0aW9ucyA9IEhhbENsaWVudC5jcmVhdGVPcHRpb25zKGhlYWRlcnMpO1xyXG4gICAgbGV0IGR0b1Jlc3BvbnNlOiBIdHRwUmVzcG9uc2U8UmVzb3VyY2VEdG8+IHwgdW5kZWZpbmVkO1xyXG4gICAgdHJ5IHtcclxuICAgICAgZHRvUmVzcG9uc2UgPSBhd2FpdCB0aGlzLl9odHRwQ2xpZW50LmdldDxSZXNvdXJjZUR0bz4odXJpLCBvcHRpb25zKS50b1Byb21pc2UoKTtcclxuICAgIH1cclxuICAgIGNhdGNoIChlKSB7XHJcbiAgICAgIGlmIChlIGluc3RhbmNlb2YgSHR0cEVycm9yUmVzcG9uc2UpXHJcbiAgICAgICAgZHRvUmVzcG9uc2UgPSBIYWxDbGllbnQuY29udmVydEVycm9yUmVzcG9uc2UoZSk7XHJcbiAgICAgIGVsc2VcclxuICAgICAgICB0aHJvdyBuZXcgRXJyb3IoYEdFVCAke3VyaX0gLSBvcHRpb25zOiAke29wdGlvbnN9IGZhaWxlZCB3aXRoIGVycm9yICR7ZX1gKTtcclxuICAgIH1cclxuICAgIGlmICghZHRvUmVzcG9uc2UpXHJcbiAgICAgIHRocm93IG5ldyBFcnJvcihgR0VUICR7dXJpfSAtIG9wdGlvbnM6ICR7b3B0aW9uc30gZGlkIG5vdCByZXR1cm4gYSByZXNwb25zZS5gKTtcclxuICAgIGNvbnN0IHJlc291cmNlUmVzcG9uc2UgPSBIYWxDbGllbnQuY29udmVydFJlc3BvbnNlIDxUUmVzb3VyY2UgfCBURXJyb3I+KGR0b1Jlc3BvbnNlLm9rID8gVFJlc291cmNlIDogVEVycm9yLCBkdG9SZXNwb25zZSk7XHJcbiAgICByZXR1cm4gcmVzb3VyY2VSZXNwb25zZTtcclxuICB9XHJcblxyXG4gIHB1YmxpYyBhc3luYyBwb3N0PFRSZXNvdXJjZSBleHRlbmRzIFJlc291cmNlLCBURXJyb3IgZXh0ZW5kcyBSZXNvdXJjZT4odXJpOiBzdHJpbmcsIGJvZHk6IFJlc291cmNlLCBUUmVzb3VyY2U6IHsgbmV3KCk6IFRSZXNvdXJjZSB9LCBURXJyb3I6IHsgbmV3KCk6IFRFcnJvciB9LCBoZWFkZXJzPzogSHR0cEhlYWRlcnMpOiBQcm9taXNlPEh0dHBSZXNwb25zZTxUUmVzb3VyY2UgfCBURXJyb3I+PiB7XHJcbiAgICBjb25zdCBvcHRpb25zID0gSGFsQ2xpZW50LmNyZWF0ZU9wdGlvbnMoaGVhZGVycyk7XHJcbiAgICBsZXQgZHRvUmVzcG9uc2U6IEh0dHBSZXNwb25zZTxSZXNvdXJjZUR0bz4gfCB1bmRlZmluZWQ7XHJcbiAgICB0cnkge1xyXG4gICAgICBkdG9SZXNwb25zZSA9IGF3YWl0IHRoaXMuX2h0dHBDbGllbnQucG9zdDxUUmVzb3VyY2U+KHVyaSwgYm9keSwgb3B0aW9ucykudG9Qcm9taXNlKCk7XHJcbiAgICB9XHJcbiAgICBjYXRjaCAoZSkge1xyXG4gICAgICBpZiAoZSBpbnN0YW5jZW9mIEh0dHBFcnJvclJlc3BvbnNlKVxyXG4gICAgICAgIGR0b1Jlc3BvbnNlID0gSGFsQ2xpZW50LmNvbnZlcnRFcnJvclJlc3BvbnNlKGUpO1xyXG4gICAgICBlbHNlXHJcbiAgICAgICAgdGhyb3cgbmV3IEVycm9yKGBQT1NUICR7dXJpfSAtIG9wdGlvbnM6ICR7b3B0aW9uc30gLSBib2R5OiAke2JvZHl9IGZhaWxlZCB3aXRoIGVycm9yICR7ZX1gKTtcclxuICAgIH1cclxuICAgIGlmICghZHRvUmVzcG9uc2UpXHJcbiAgICAgIHRocm93IG5ldyBFcnJvcihgUE9TVCAke3VyaX0gLSBvcHRpb25zOiAke29wdGlvbnN9IC0gYm9keTogJHtib2R5fSBkaWQgbm90IHJldHVybiBhIHJlc3BvbnNlLmApO1xyXG4gICAgY29uc3QgcmVzb3VyY2VSZXNwb25zZSA9IEhhbENsaWVudC5jb252ZXJ0UmVzcG9uc2U8VFJlc291cmNlIHwgVEVycm9yPihkdG9SZXNwb25zZS5vayA/IFRSZXNvdXJjZSA6IFRFcnJvciwgZHRvUmVzcG9uc2UpO1xyXG4gICAgcmV0dXJuIHJlc291cmNlUmVzcG9uc2U7XHJcbiAgfVxyXG5cclxuICBwdWJsaWMgYXN5bmMgcHV0PFRSZXNvdXJjZSBleHRlbmRzIFJlc291cmNlLCBURXJyb3IgZXh0ZW5kcyBSZXNvdXJjZT4odXJpOiBzdHJpbmcsIGJvZHk6IFJlc291cmNlLCBUUmVzb3VyY2U6IHsgbmV3KCk6IFRSZXNvdXJjZSB9LCBURXJyb3I6IHsgbmV3KCk6IFRFcnJvciB9LCBoZWFkZXJzPzogSHR0cEhlYWRlcnMpOiBQcm9taXNlPEh0dHBSZXNwb25zZTxUUmVzb3VyY2UgfCBURXJyb3I+PiB7XHJcbiAgICBjb25zdCBvcHRpb25zID0gSGFsQ2xpZW50LmNyZWF0ZU9wdGlvbnMoaGVhZGVycyk7XHJcbiAgICBsZXQgZHRvUmVzcG9uc2U6IEh0dHBSZXNwb25zZTxSZXNvdXJjZUR0bz4gfCB1bmRlZmluZWQ7XHJcbiAgICB0cnkge1xyXG4gICAgICBkdG9SZXNwb25zZSA9IGF3YWl0IHRoaXMuX2h0dHBDbGllbnQucHV0PFRSZXNvdXJjZT4odXJpLCBib2R5LCBvcHRpb25zKS50b1Byb21pc2UoKTtcclxuICAgIH1cclxuICAgIGNhdGNoIChlKSB7XHJcbiAgICAgIGlmIChlIGluc3RhbmNlb2YgSHR0cEVycm9yUmVzcG9uc2UpXHJcbiAgICAgICAgZHRvUmVzcG9uc2UgPSBIYWxDbGllbnQuY29udmVydEVycm9yUmVzcG9uc2UoZSk7XHJcbiAgICAgIGVsc2VcclxuICAgICAgICB0aHJvdyBuZXcgRXJyb3IoYFBVVCAke3VyaX0gLSBvcHRpb25zOiAke29wdGlvbnN9IC0gYm9keTogJHtib2R5fSBmYWlsZWQgd2l0aCBlcnJvciAke2V9YCk7XHJcbiAgICB9XHJcbiAgICBpZiAoIWR0b1Jlc3BvbnNlKVxyXG4gICAgICB0aHJvdyBuZXcgRXJyb3IoYFBVVCAke3VyaX0gLSBvcHRpb25zOiAke29wdGlvbnN9IC0gYm9keTogJHtib2R5fSBkaWQgbm90IHJldHVybiBhIHJlc3BvbnNlLmApO1xyXG4gICAgY29uc3QgcmVzb3VyY2VSZXNwb25zZSA9IEhhbENsaWVudC5jb252ZXJ0UmVzcG9uc2U8VFJlc291cmNlIHwgVEVycm9yPihkdG9SZXNwb25zZS5vayA/IFRSZXNvdXJjZSA6IFRFcnJvciwgZHRvUmVzcG9uc2UpO1xyXG4gICAgcmV0dXJuIHJlc291cmNlUmVzcG9uc2U7XHJcbiAgfVxyXG5cclxuICBwdWJsaWMgYXN5bmMgZGVsZXRlPFRFcnJvciBleHRlbmRzIFJlc291cmNlPih1cmk6IHN0cmluZywgVEVycm9yOiB7IG5ldygpOiBURXJyb3IgfSwgaGVhZGVycz86IEh0dHBIZWFkZXJzKTogUHJvbWlzZTxIdHRwUmVzcG9uc2U8dm9pZCB8IFRFcnJvcj4+IHtcclxuICAgIGNvbnN0IG9wdGlvbnMgPSBIYWxDbGllbnQuY3JlYXRlT3B0aW9ucyhoZWFkZXJzKTtcclxuICAgIGxldCByZXNwb25zZTogSHR0cFJlc3BvbnNlPGFueT4gfCB1bmRlZmluZWQ7XHJcbiAgICB0cnkge1xyXG4gICAgICByZXNwb25zZSA9IGF3YWl0IHRoaXMuX2h0dHBDbGllbnQuZGVsZXRlPEh0dHBSZXNwb25zZTx2b2lkPj4odXJpLCBvcHRpb25zKS50b1Byb21pc2UoKTtcclxuICAgIH1cclxuICAgIGNhdGNoIChlKSB7XHJcbiAgICAgIGlmIChlIGluc3RhbmNlb2YgSHR0cEVycm9yUmVzcG9uc2UpXHJcbiAgICAgICAgcmVzcG9uc2UgPSBIYWxDbGllbnQuY29udmVydEVycm9yUmVzcG9uc2UoZSk7XHJcbiAgICAgIGVsc2VcclxuICAgICAgICB0aHJvdyBuZXcgRXJyb3IoYERFTEVURSAke3VyaX0gLSBvcHRpb25zOiAke29wdGlvbnN9IGZhaWxlZCB3aXRoIGVycm9yICR7ZX1gKTtcclxuICAgIH1cclxuICAgIGlmICghcmVzcG9uc2UpXHJcbiAgICAgIHRocm93IG5ldyBFcnJvcihgREVMRVRFICR7dXJpfSAtIG9wdGlvbnM6ICR7b3B0aW9uc30gZGlkIG5vdCByZXR1cm4gYSByZXNwb25zZS5gKTtcclxuICAgIGlmICghcmVzcG9uc2Uub2spIHtcclxuICAgICAgY29uc3QgZXJyb3JSZXNwb25zZSA9IEhhbENsaWVudC5jb252ZXJ0UmVzcG9uc2U8VEVycm9yPihURXJyb3IsIHJlc3BvbnNlKTtcclxuICAgICAgcmV0dXJuIGVycm9yUmVzcG9uc2U7XHJcbiAgICB9XHJcblxyXG4gICAgcmV0dXJuIHJlc3BvbnNlO1xyXG4gIH1cclxuXHJcbiAgcHJpdmF0ZSBzdGF0aWMgY3JlYXRlT3B0aW9ucyhoZWFkZXJzPzogSHR0cEhlYWRlcnMpOiB7IGhlYWRlcnM/OiBIdHRwSGVhZGVycywgcmVzcG9uc2VUeXBlOiAnanNvbicsIG9ic2VydmU6ICdyZXNwb25zZScgfSB7XHJcbiAgICBoZWFkZXJzPy5hcHBlbmQoJ0FjY2VwdCcsICdhcHBsaWNhdGlvbi9oYWwranNvbicpXHJcbiAgICByZXR1cm4ge1xyXG4gICAgICBoZWFkZXJzOiBoZWFkZXJzLFxyXG4gICAgICByZXNwb25zZVR5cGU6ICdqc29uJyxcclxuICAgICAgb2JzZXJ2ZTogJ3Jlc3BvbnNlJ1xyXG4gICAgfVxyXG4gIH1cclxuXHJcbiAgcHVibGljIHN0YXRpYyBjb252ZXJ0UmVzcG9uc2U8VFJlc291cmNlIGV4dGVuZHMgUmVzb3VyY2U+KFRSZXNvdXJjZTogeyBuZXcoKTogVFJlc291cmNlIH0sIHJlc3BvbnNlOiBIdHRwUmVzcG9uc2U8UmVzb3VyY2VEdG8+KTogSHR0cFJlc3BvbnNlPFRSZXNvdXJjZT4ge1xyXG4gICAgY29uc3QgcmVzb3VyY2UgPSBSZXNvdXJjZS5mcm9tRHRvKHJlc3BvbnNlLmJvZHkgfHwgbmV3IFRSZXNvdXJjZSgpLCBUUmVzb3VyY2UpO1xyXG4gICAgY29uc3QgcmVzb3VyY2VSZXNwb25zZSA9IHJlc3BvbnNlLmNsb25lPFRSZXNvdXJjZT4oeyBib2R5OiByZXNvdXJjZSB9KTtcclxuICAgIHJldHVybiByZXNvdXJjZVJlc3BvbnNlO1xyXG4gIH1cclxuXHJcbiAgcHJpdmF0ZSBzdGF0aWMgY29udmVydEVycm9yUmVzcG9uc2U8VFJlc291cmNlRHRvPihlOiBIdHRwRXJyb3JSZXNwb25zZSk6IEh0dHBSZXNwb25zZTxUUmVzb3VyY2VEdG8+IHtcclxuICAgIGNvbnN0IGR0b1Jlc3BvbnNlID0gbmV3IEh0dHBSZXNwb25zZSh7IGJvZHk6IGUuZXJyb3IsIGhlYWRlcnM6IGUuaGVhZGVycywgc3RhdHVzOiBlLnN0YXR1cywgc3RhdHVzVGV4dDogZS5zdGF0dXNUZXh0LCB1cmw6IGUudXJsID8gZS51cmwgOiB1bmRlZmluZWQgfSk7XHJcbiAgICByZXR1cm4gZHRvUmVzcG9uc2U7XHJcbiAgfVxyXG59XHJcbiJdfQ==