import { __awaiter } from "tslib";
import { HttpErrorResponse, HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
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
        const resource = new TResource(response.body);
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
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoiaGFsLWNsaWVudC5qcyIsInNvdXJjZVJvb3QiOiIiLCJzb3VyY2VzIjpbIi4uLy4uLy4uLy4uL3Byb2plY3RzL25neC1oYWwtY2xpZW50L3NyYy9saWIvaGFsLWNsaWVudC50cyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiO0FBQUEsT0FBTyxFQUFjLGlCQUFpQixFQUFlLFlBQVksRUFBRSxNQUFNLHNCQUFzQixDQUFDO0FBQ2hHLE9BQU8sRUFBRSxVQUFVLEVBQUUsTUFBTSxlQUFlLENBQUM7OztBQU0zQyxNQUFNLE9BQU8sU0FBUztJQUVwQixZQUFvQixXQUF1QjtRQUF2QixnQkFBVyxHQUFYLFdBQVcsQ0FBWTtJQUFJLENBQUM7SUFFbkMsR0FBRyxDQUFzRCxHQUFXLEVBQUUsU0FBK0IsRUFBRSxNQUF5QixFQUFFLE9BQXFCOztZQUNsSyxNQUFNLE9BQU8sR0FBRyxTQUFTLENBQUMsYUFBYSxDQUFDLE9BQU8sQ0FBQyxDQUFDO1lBQ2pELElBQUksV0FBa0QsQ0FBQztZQUN2RCxJQUFJO2dCQUNGLFdBQVcsR0FBRyxNQUFNLElBQUksQ0FBQyxXQUFXLENBQUMsR0FBRyxDQUFjLEdBQUcsRUFBRSxPQUFPLENBQUMsQ0FBQyxTQUFTLEVBQUUsQ0FBQzthQUNqRjtZQUNELE9BQU8sQ0FBQyxFQUFFO2dCQUNSLElBQUksQ0FBQyxZQUFZLGlCQUFpQjtvQkFDaEMsV0FBVyxHQUFHLFNBQVMsQ0FBQyxvQkFBb0IsQ0FBQyxDQUFDLENBQUMsQ0FBQzs7b0JBRWhELE1BQU0sSUFBSSxLQUFLLENBQUMsT0FBTyxHQUFHLGVBQWUsT0FBTyxzQkFBc0IsQ0FBQyxFQUFFLENBQUMsQ0FBQzthQUM5RTtZQUNELElBQUksQ0FBQyxXQUFXO2dCQUNkLE1BQU0sSUFBSSxLQUFLLENBQUMsT0FBTyxHQUFHLGVBQWUsT0FBTyw2QkFBNkIsQ0FBQyxDQUFDO1lBQ2pGLE1BQU0sZ0JBQWdCLEdBQUcsU0FBUyxDQUFDLGVBQWUsQ0FBc0IsV0FBVyxDQUFDLEVBQUUsQ0FBQyxDQUFDLENB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LE9BQU8sRUFBRSxNQUFNLEVBQUUsQ0FBQyxDQUFDLE1BQU0sRUFBRSxVQUFVLEVBQUUsQ0FBQyxDQUFDLFVBQVUsRUFBRSxHQUFHLEVBQUUsQ0FBQyxDQUFDLEdBQUcsQ0FBQyxDQUFDLENBQUMsQ0FBQyxDQUFDLEdBQUcsQ0FBQyxDQUFDLENBQUMsU0FBUyxFQUFFLENBQUMsQ0FBQztRQUN4SixPQUFPLFdBQVcsQ0FBQztJQUNyQixDQUFDOzt1R0FsR1UsU0FBUzsyR0FBVCxTQUFTLGNBRlIsTUFBTTs0RkFFUCxTQUFTO2tCQUhyQixVQUFVO21CQUFDO29CQUNWLFVBQVUsRUFBRSxNQUFNO2lCQUNuQiIsInNvdXJjZXNDb250ZW50IjpbImltcG9ydCB7IEh0dHBDbGllbnQsIEh0dHBFcnJvclJlc3BvbnNlLCBIdHRwSGVhZGVycywgSHR0cFJlc3BvbnNlIH0gZnJvbSAnQGFuZ3VsYXIvY29tbW9uL2h0dHAnO1xyXG5pbXBvcnQgeyBJbmplY3RhYmxlIH0gZnJvbSAnQGFuZ3VsYXIvY29yZSc7XHJcbmltcG9ydCB7IFJlc291cmNlLCBSZXNvdXJjZUR0byB9IGZyb20gJy4vTW9kZWxzL3Jlc291cmNlJztcclxuXHJcbkBJbmplY3RhYmxlKHtcclxuICBwcm92aWRlZEluOiAncm9vdCdcclxufSlcclxuZXhwb3J0IGNsYXNzIEhhbENsaWVudCB7XHJcblxyXG4gIGNvbnN0cnVjdG9yKHByaXZhdGUgX2h0dHBDbGllbnQ6IEh0dHBDbGllbnQpIHsgfVxyXG5cclxuICBwdWJsaWMgYXN5bmMgZ2V0PFRSZXNvdXJjZSBleHRlbmRzIFJlc291cmNlLCBURXJyb3IgZXh0ZW5kcyBSZXNvdXJjZT4odXJpOiBzdHJpbmcsIFRSZXNvdXJjZTogeyBuZXcoKTogVFJlc291cmNlIH0sIFRFcnJvcjogeyBuZXcoKTogVEVycm9yIH0sIGhlYWRlcnM/OiBIdHRwSGVhZGVycyk6IFByb21pc2U8SHR0cFJlc3BvbnNlPFRSZXNvdXJjZSB8IFRFcnJvcj4+IHtcclxuICAgIGNvbnN0IG9wdGlvbnMgPSBIYWxDbGllbnQuY3JlYXRlT3B0aW9ucyhoZWFkZXJzKTtcclxuICAgIGxldCBkdG9SZXNwb25zZTogSHR0cFJlc3BvbnNlPFJlc291cmNlRHRvPiB8IHVuZGVmaW5lZDtcclxuICAgIHRyeSB7XHJcbiAgICAgIGR0b1Jlc3BvbnNlID0gYXdhaXQgdGhpcy5faHR0cENsaWVudC5nZXQ8UmVzb3VyY2VEdG8+KHVyaSwgb3B0aW9ucykudG9Qcm9taXNlKCk7XHJcbiAgICB9XHJcbiAgICBjYXRjaCAoZSkge1xyXG4gICAgICBpZiAoZSBpbnN0YW5jZW9mIEh0dHBFcnJvclJlc3BvbnNlKVxyXG4gICAgICAgIGR0b1Jlc3BvbnNlID0gSGFsQ2xpZW50LmNvbnZlcnRFcnJvclJlc3BvbnNlKGUpO1xyXG4gICAgICBlbHNlXHJcbiAgICAgICAgdGhyb3cgbmV3IEVycm9yKGBHRVQgJHt1cml9IC0gb3B0aW9uczogJHtvcHRpb25zfSBmYWlsZWQgd2l0aCBlcnJvciAke2V9YCk7XHJcbiAgICB9XHJcbiAgICBpZiAoIWR0b1Jlc3BvbnNlKVxyXG4gICAgICB0aHJvdyBuZXcgRXJyb3IoYEdFVCAke3VyaX0gLSBvcHRpb25zOiAke29wdGlvbnN9IGRpZCBub3QgcmV0dXJuIGEgcmVzcG9uc2UuYCk7XHJcbiAgICBjb25zdCByZXNvdXJjZVJlc3BvbnNlID0gSGFsQ2xpZW50LmNvbnZlcnRSZXNwb25zZSA8VFJlc291cmNlIHwgVEVycm9yPihkdG9SZXNwb25zZS5vayA/IFRSZXNvdXJjZSA6IFRFcnJvciwgZHRvUmVzcG9uc2UpO1xyXG4gICAgcmV0dXJuIHJlc291cmNlUmVzcG9uc2U7XHJcbiAgfVxyXG5cclxuICBwdWJsaWMgYXN5bmMgcG9zdDxUUmVzb3VyY2UgZXh0ZW5kcyBSZXNvdXJjZSwgVEVycm9yIGV4dGVuZHMgUmVzb3VyY2U+KHVyaTogc3RyaW5nLCBib2R5OiB1bmtub3duLCBUUmVzb3VyY2U6IHsgbmV3KCk6IFRSZXNvdXJjZSB9LCBURXJyb3I6IHsgbmV3KCk6IFRFcnJvciB9LCBoZWFkZXJzPzogSHR0cEhlYWRlcnMpOiBQcm9taXNlPEh0dHBSZXNwb25zZTxUUmVzb3VyY2UgfCBURXJyb3I+PiB7XHJcbiAgICBjb25zdCBvcHRpb25zID0gSGFsQ2xpZW50LmNyZWF0ZU9wdGlvbnMoaGVhZGVycyk7XHJcbiAgICBsZXQgZHRvUmVzcG9uc2U6IEh0dHBSZXNwb25zZTxSZXNvdXJjZUR0bz4gfCB1bmRlZmluZWQ7XHJcbiAgICB0cnkge1xyXG4gICAgICBkdG9SZXNwb25zZSA9IGF3YWl0IHRoaXMuX2h0dHBDbGllbnQucG9zdDxUUmVzb3VyY2U+KHVyaSwgYm9keSwgb3B0aW9ucykudG9Qcm9taXNlKCk7XHJcbiAgICB9XHJcbiAgICBjYXRjaCAoZSkge1xyXG4gICAgICBpZiAoZSBpbnN0YW5jZW9mIEh0dHBFcnJvclJlc3BvbnNlKVxyXG4gICAgICAgIGR0b1Jlc3BvbnNlID0gSGFsQ2xpZW50LmNvbnZlcnRFcnJvclJlc3BvbnNlKGUpO1xyXG4gICAgICBlbHNlXHJcbiAgICAgICAgdGhyb3cgbmV3IEVycm9yKGBQT1NUICR7dXJpfSAtIG9wdGlvbnM6ICR7b3B0aW9uc30gLSBib2R5OiAke2JvZHl9IGZhaWxlZCB3aXRoIGVycm9yICR7ZX1gKTtcclxuICAgIH1cclxuICAgIGlmICghZHRvUmVzcG9uc2UpXHJcbiAgICAgIHRocm93IG5ldyBFcnJvcihgUE9TVCAke3VyaX0gLSBvcHRpb25zOiAke29wdGlvbnN9IC0gYm9keTogJHtib2R5fSBkaWQgbm90IHJldHVybiBhIHJlc3BvbnNlLmApO1xyXG4gICAgY29uc3QgcmVzb3VyY2VSZXNwb25zZSA9IEhhbENsaWVudC5jb252ZXJ0UmVzcG9uc2U8VFJlc291cmNlIHwgVEVycm9yPihkdG9SZXNwb25zZS5vayA/IFRSZXNvdXJjZSA6IFRFcnJvciwgZHRvUmVzcG9uc2UpO1xyXG4gICAgcmV0dXJuIHJlc291cmNlUmVzcG9uc2U7XHJcbiAgfVxyXG5cclxuICBwdWJsaWMgYXN5bmMgcHV0PFRSZXNvdXJjZSBleHRlbmRzIFJlc291cmNlLCBURXJyb3IgZXh0ZW5kcyBSZXNvdXJjZT4odXJpOiBzdHJpbmcsIGJvZHk6IHVua25vd24sIFRSZXNvdXJjZTogeyBuZXcoKTogVFJlc291cmNlIH0sIFRFcnJvcjogeyBuZXcoKTogVEVycm9yIH0sIGhlYWRlcnM/OiBIdHRwSGVhZGVycyk6IFByb21pc2U8SHR0cFJlc3BvbnNlPFRSZXNvdXJjZSB8IFRFcnJvcj4+IHtcclxuICAgIGNvbnN0IG9wdGlvbnMgPSBIYWxDbGllbnQuY3JlYXRlT3B0aW9ucyhoZWFkZXJzKTtcclxuICAgIGxldCBkdG9SZXNwb25zZTogSHR0cFJlc3BvbnNlPFJlc291cmNlRHRvPiB8IHVuZGVmaW5lZDtcclxuICAgIHRyeSB7XHJcbiAgICAgIGR0b1Jlc3BvbnNlID0gYXdhaXQgdGhpcy5faHR0cENsaWVudC5wdXQ8VFJlc291cmNlPih1cmksIGJvZHksIG9wdGlvbnMpLnRvUHJvbWlzZSgpO1xyXG4gICAgfVxyXG4gICAgY2F0Y2ggKGUpIHtcclxuICAgICAgaWYgKGUgaW5zdGFuY2VvZiBIdHRwRXJyb3JSZXNwb25zZSlcclxuICAgICAgICBkdG9SZXNwb25zZSA9IEhhbENsaWVudC5jb252ZXJ0RXJyb3JSZXNwb25zZShlKTtcclxuICAgICAgZWxzZVxyXG4gICAgICAgIHRocm93IG5ldyBFcnJvcihgUFVUICR7dXJpfSAtIG9wdGlvbnM6ICR7b3B0aW9uc30gLSBib2R5OiAke2JvZHl9IGZhaWxlZCB3aXRoIGVycm9yICR7ZX1gKTtcclxuICAgIH1cclxuICAgIGlmICghZHRvUmVzcG9uc2UpXHJcbiAgICAgIHRocm93IG5ldyBFcnJvcihgUFVUICR7dXJpfSAtIG9wdGlvbnM6ICR7b3B0aW9uc30gLSBib2R5OiAke2JvZHl9IGRpZCBub3QgcmV0dXJuIGEgcmVzcG9uc2UuYCk7XHJcbiAgICBjb25zdCByZXNvdXJjZVJlc3BvbnNlID0gSGFsQ2xpZW50LmNvbnZlcnRSZXNwb25zZTxUUmVzb3VyY2UgfCBURXJyb3I+KGR0b1Jlc3BvbnNlLm9rID8gVFJlc291cmNlIDogVEVycm9yLCBkdG9SZXNwb25zZSk7XHJcbiAgICByZXR1cm4gcmVzb3VyY2VSZXNwb25zZTtcclxuICB9XHJcblxyXG4gIHB1YmxpYyBhc3luYyBkZWxldGU8VEVycm9yIGV4dGVuZHMgUmVzb3VyY2U+KHVyaTogc3RyaW5nLCBURXJyb3I6IHsgbmV3KCk6IFRFcnJvciB9LCBoZWFkZXJzPzogSHR0cEhlYWRlcnMpOiBQcm9taXNlPEh0dHBSZXNwb25zZTx2b2lkIHwgVEVycm9yPj4ge1xyXG4gICAgY29uc3Qgb3B0aW9ucyA9IEhhbENsaWVudC5jcmVhdGVPcHRpb25zKGhlYWRlcnMpO1xyXG4gICAgbGV0IHJlc3BvbnNlOiBIdHRwUmVzcG9uc2U8UmVzb3VyY2VEdG8gfCB2b2lkPiB8IHVuZGVmaW5lZDtcclxuICAgIHRyeSB7XHJcbiAgICAgIHJlc3BvbnNlID0gYXdhaXQgdGhpcy5faHR0cENsaWVudC5kZWxldGU8dm9pZD4odXJpLCBvcHRpb25zKS50b1Byb21pc2UoKTtcclxuICAgIH1cclxuICAgIGNhdGNoIChlKSB7XHJcbiAgICAgIGlmIChlIGluc3RhbmNlb2YgSHR0cEVycm9yUmVzcG9uc2UpXHJcbiAgICAgICAgcmVzcG9uc2UgPSBIYWxDbGllbnQuY29udmVydEVycm9yUmVzcG9uc2UoZSk7XHJcbiAgICAgIGVsc2VcclxuICAgICAgICB0aHJvdyBuZXcgRXJyb3IoYERFTEVURSAke3VyaX0gLSBvcHRpb25zOiAke29wdGlvbnN9IGZhaWxlZCB3aXRoIGVycm9yICR7ZX1gKTtcclxuICAgIH1cclxuICAgIGlmICghcmVzcG9uc2UpXHJcbiAgICAgIHRocm93IG5ldyBFcnJvcihgREVMRVRFICR7dXJpfSAtIG9wdGlvbnM6ICR7b3B0aW9uc30gZGlkIG5vdCByZXR1cm4gYSByZXNwb25zZS5gKTtcclxuICAgIGlmICghcmVzcG9uc2Uub2spIHtcclxuICAgICAgY29uc3QgZXJyb3JSZXNwb25zZSA9IEhhbENsaWVudC5jb252ZXJ0UmVzcG9uc2U8VEVycm9yPihURXJyb3IsIHJlc3BvbnNlIGFzIEh0dHBSZXNwb25zZTxSZXNvdXJjZUR0bz4pO1xyXG4gICAgICByZXR1cm4gZXJyb3JSZXNwb25zZTtcclxuICAgIH1cclxuXHJcbiAgICByZXR1cm4gcmVzcG9uc2UgYXMgSHR0cFJlc3BvbnNlPHZvaWQ+O1xyXG4gIH1cclxuXHJcbiAgcHJpdmF0ZSBzdGF0aWMgY3JlYXRlT3B0aW9ucyhoZWFkZXJzPzogSHR0cEhlYWRlcnMpOiB7IGhlYWRlcnM/OiBIdHRwSGVhZGVyczsgcmVzcG9uc2VUeXBlOiAnanNvbic7IG9ic2VydmU6ICdyZXNwb25zZScgfSB7XHJcbiAgICBoZWFkZXJzPy5hcHBlbmQoJ0FjY2VwdCcsICdhcHBsaWNhdGlvbi9oYWwranNvbicpXHJcbiAgICByZXR1cm4ge1xyXG4gICAgICBoZWFkZXJzOiBoZWFkZXJzLFxyXG4gICAgICByZXNwb25zZVR5cGU6ICdqc29uJyxcclxuICAgICAgb2JzZXJ2ZTogJ3Jlc3BvbnNlJ1xyXG4gICAgfVxyXG4gIH1cclxuXHJcbiAgcHVibGljIHN0YXRpYyBjb252ZXJ0UmVzcG9uc2U8VFJlc291cmNlIGV4dGVuZHMgUmVzb3VyY2U+KFRSZXNvdXJjZTogeyBuZXcoZHRvPzogUmVzb3VyY2VEdG8gfCBudWxsKTogVFJlc291cmNlOyB9LCByZXNwb25zZTogSHR0cFJlc3BvbnNlPFJlc291cmNlRHRvPik6IEh0dHBSZXNwb25zZTxUUmVzb3VyY2U+IHtcclxuICAgIGNvbnN0IHJlc291cmNlID0gbmV3IFRSZXNvdXJjZShyZXNwb25zZS5ib2R5KTtcclxuICAgIGNvbnN0IHJlc291cmNlUmVzcG9uc2UgPSByZXNwb25zZS5jbG9uZTxUUmVzb3VyY2U+KHsgYm9keTogcmVzb3VyY2UgfSk7XHJcbiAgICByZXR1cm4gcmVzb3VyY2VSZXNwb25zZTtcclxuICB9XHJcblxyXG4gIHByaXZhdGUgc3RhdGljIGNvbnZlcnRFcnJvclJlc3BvbnNlPFRSZXNvdXJjZUR0bz4oZTogSHR0cEVycm9yUmVzcG9uc2UpOiBIdHRwUmVzcG9uc2U8VFJlc291cmNlRHRvPiB7XHJcbiAgICBjb25zdCBkdG9SZXNwb25zZSA9IG5ldyBIdHRwUmVzcG9uc2UoeyBib2R5OiBlLmVycm9yLCBoZWFkZXJzOiBlLmhlYWRlcnMsIHN0YXR1czogZS5zdGF0dXMsIHN0YXR1c1RleHQ6IGUuc3RhdHVzVGV4dCwgdXJsOiBlLnVybCA/IGUudXJsIDogdW5kZWZpbmVkIH0pO1xyXG4gICAgcmV0dXJuIGR0b1Jlc3BvbnNlO1xyXG4gIH1cclxufVxyXG4iXX0=