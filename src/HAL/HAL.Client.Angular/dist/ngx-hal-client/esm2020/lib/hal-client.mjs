import { HttpErrorResponse, HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import * as i0 from "@angular/core";
import * as i1 from "@angular/common/http";
export class HalClient {
    constructor(_httpClient) {
        this._httpClient = _httpClient;
    }
    async get(uri, TResource, TError, headers) {
        const options = HalClient.createOptions(headers);
        let dtoResponse;
        try {
            dtoResponse = await this._httpClient.get(uri, options).toPromise();
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
    }
    async post(uri, body, TResource, TError, headers) {
        const options = HalClient.createOptions(headers);
        let dtoResponse;
        try {
            dtoResponse = await this._httpClient.post(uri, body, options).toPromise();
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
    }
    async put(uri, body, TResource, TError, headers) {
        const options = HalClient.createOptions(headers);
        let dtoResponse;
        try {
            dtoResponse = await this._httpClient.put(uri, body, options).toPromise();
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
    }
    async delete(uri, TError, headers) {
        const options = HalClient.createOptions(headers);
        let response;
        try {
            response = await this._httpClient.delete(uri, options).toPromise();
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
    }
    static createOptions(headers) {
        headers?.append('Accept', 'application/hal+json');
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
HalClient.ɵfac = i0.ɵɵngDeclareFactory({ minVersion: "12.0.0", version: "13.2.5", ngImport: i0, type: HalClient, deps: [{ token: i1.HttpClient }], target: i0.ɵɵFactoryTarget.Injectable });
HalClient.ɵprov = i0.ɵɵngDeclareInjectable({ minVersion: "12.0.0", version: "13.2.5", ngImport: i0, type: HalClient, providedIn: 'root' });
i0.ɵɵngDeclareClassMetadata({ minVersion: "12.0.0", version: "13.2.5", ngImport: i0, type: HalClient, decorators: [{
            type: Injectable,
            args: [{
                    providedIn: 'root'
                }]
        }], ctorParameters: function () { return [{ type: i1.HttpClient }]; } });
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoiaGFsLWNsaWVudC5qcyIsInNvdXJjZVJvb3QiOiIiLCJzb3VyY2VzIjpbIi4uLy4uLy4uLy4uL3Byb2plY3RzL25neC1oYWwtY2xpZW50L3NyYy9saWIvaGFsLWNsaWVudC50cyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFBQSxPQUFPLEVBQWMsaUJBQWlCLEVBQWUsWUFBWSxFQUFFLE1BQU0sc0JBQXNCLENBQUM7QUFDaEcsT0FBTyxFQUFFLFVBQVUsRUFBRSxNQUFNLGVBQWUsQ0FBQzs7O0FBTTNDLE1BQU0sT0FBTyxTQUFTO0lBRXBCLFlBQW9CLFdBQXVCO1FBQXZCLGdCQUFXLEdBQVgsV0FBVyxDQUFZO0lBQUksQ0FBQztJQUV6QyxLQUFLLENBQUMsR0FBRyxDQUFzRCxHQUFXLEVBQUUsU0FBK0IsRUFBRSxNQUF5QixFQUFFLE9BQXFCO1FBQ2xLLE1BQU0sT0FBTyxHQUFHLFNBQVMsQ0FBQyxhQUFhLENBQUMsT0FBTyxDQUFDLENBQUM7UUFDakQsSUFBSSxXQUFrRCxDQUFDO1FBQ3ZELElBQUk7WUFDRixXQUFXLEdBQUcsTUFBTSxJQUFJLENBQUMsV0FBVyxDQUFDLEdBQUcsQ0FBYyxHQUFHLEVBQUUsT0FBTyxDQUFDLENBQUMsU0FBUyxFQUFFLENBQUM7U0FDakY7UUFDRCxPQUFPLENBQUMsRUFBRTtZQUNSLElBQUksQ0FBQyxZQUFZLGlCQUFpQjtnQkFDaEMsV0FBVyxHQUFHLFNBQVMsQ0FBQyxvQkFBb0IsQ0FBQyxDQUFDLENBQUMsQ0FBQzs7Z0JBRWhELE1BQU0sSUFBSSxLQUFLLENBQUMsT0FBTyxHQUFHLGVBQWUsT0FBTyxzQkFBc0IsQ0FBQyxFQUFFLENBQUMsQ0FBQztTQUM5RTtRQUNELElBQUksQ0FBQyxXQUFXO1lBQ2QsTUFBTSxJQUFJLEtBQUssQ0FBQyxPQUFPLEdBQUcsZUFBZSxPQUFPLDZCQUE2QixDQUFDLENBQUM7UUFDakYsTUFBTSxnQkFBZ0IsR0FBRyxTQUFTLENBQUMsZUFBZSxDQUFzQixXQUFXLENBQUMsRUFBRSxDQUFDLENB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LENBQUMsQ0FBQyxNQUFNLEVBQUUsVUFBVSxFQUFFLENBQUMsQ0FBQyxVQUFVLEVBQUUsR0FBRyxFQUFFLENBQUMsQ0FBQyxHQUFHLENBQUMsQ0FBQyxDQUFDLENBQUMsQ0FBQyxHQUFHLENBQUMsQ0FBQyxDQUFDLFNBQVMsRUFBRSxDQUFDLENBQUM7UUFDeEosT0FBTyxXQUFXLENBQUM7SUFDckIsQ0FBQzs7c0dBbEdVLFNBQVM7MEdBQVQsU0FBUyxjQUZSLE1BQU07MkZBRVAsU0FBUztrQkFIckIsVUFBVTttQkFBQztvQkFDVixVQUFVLEVBQUUsTUFBTTtpQkFDbkIiLCJzb3VyY2VzQ29udGVudCI6WyJpbXBvcnQgeyBIdHRwQ2xpZW50LCBIdHRwRXJyb3JSZXNwb25zZSwgSHR0cEhlYWRlcnMsIEh0dHBSZXNwb25zZSB9IGZyb20gJ0Bhbmd1bGFyL2NvbW1vbi9odHRwJztcclxuaW1wb3J0IHsgSW5qZWN0YWJsZSB9IGZyb20gJ0Bhbmd1bGFyL2NvcmUnO1xyXG5pbXBvcnQgeyBSZXNvdXJjZSwgUmVzb3VyY2VEdG8gfSBmcm9tICcuL01vZGVscy9yZXNvdXJjZSc7XHJcblxyXG5ASW5qZWN0YWJsZSh7XHJcbiAgcHJvdmlkZWRJbjogJ3Jvb3QnXHJcbn0pXHJcbmV4cG9ydCBjbGFzcyBIYWxDbGllbnQge1xyXG5cclxuICBjb25zdHJ1Y3Rvcihwcml2YXRlIF9odHRwQ2xpZW50OiBIdHRwQ2xpZW50KSB7IH1cclxuXHJcbiAgcHVibGljIGFzeW5jIGdldDxUUmVzb3VyY2UgZXh0ZW5kcyBSZXNvdXJjZSwgVEVycm9yIGV4dGVuZHMgUmVzb3VyY2U+KHVyaTogc3RyaW5nLCBUUmVzb3VyY2U6IHsgbmV3KCk6IFRSZXNvdXJjZSB9LCBURXJyb3I6IHsgbmV3KCk6IFRFcnJvciB9LCBoZWFkZXJzPzogSHR0cEhlYWRlcnMpOiBQcm9taXNlPEh0dHBSZXNwb25zZTxUUmVzb3VyY2UgfCBURXJyb3I+PiB7XHJcbiAgICBjb25zdCBvcHRpb25zID0gSGFsQ2xpZW50LmNyZWF0ZU9wdGlvbnMoaGVhZGVycyk7XHJcbiAgICBsZXQgZHRvUmVzcG9uc2U6IEh0dHBSZXNwb25zZTxSZXNvdXJjZUR0bz4gfCB1bmRlZmluZWQ7XHJcbiAgICB0cnkge1xyXG4gICAgICBkdG9SZXNwb25zZSA9IGF3YWl0IHRoaXMuX2h0dHBDbGllbnQuZ2V0PFJlc291cmNlRHRvPih1cmksIG9wdGlvbnMpLnRvUHJvbWlzZSgpO1xyXG4gICAgfVxyXG4gICAgY2F0Y2ggKGUpIHtcclxuICAgICAgaWYgKGUgaW5zdGFuY2VvZiBIdHRwRXJyb3JSZXNwb25zZSlcclxuICAgICAgICBkdG9SZXNwb25zZSA9IEhhbENsaWVudC5jb252ZXJ0RXJyb3JSZXNwb25zZShlKTtcclxuICAgICAgZWxzZVxyXG4gICAgICAgIHRocm93IG5ldyBFcnJvcihgR0VUICR7dXJpfSAtIG9wdGlvbnM6ICR7b3B0aW9uc30gZmFpbGVkIHdpdGggZXJyb3IgJHtlfWApO1xyXG4gICAgfVxyXG4gICAgaWYgKCFkdG9SZXNwb25zZSlcclxuICAgICAgdGhyb3cgbmV3IEVycm9yKGBHRVQgJHt1cml9IC0gb3B0aW9uczogJHtvcHRpb25zfSBkaWQgbm90IHJldHVybiBhIHJlc3BvbnNlLmApO1xyXG4gICAgY29uc3QgcmVzb3VyY2VSZXNwb25zZSA9IEhhbENsaWVudC5jb252ZXJ0UmVzcG9uc2UgPFRSZXNvdXJjZSB8IFRFcnJvcj4oZHRvUmVzcG9uc2Uub2sgPyBUUmVzb3VyY2UgOiBURXJyb3IsIGR0b1Jlc3BvbnNlKTtcclxuICAgIHJldHVybiByZXNvdXJjZVJlc3BvbnNlO1xyXG4gIH1cclxuXHJcbiAgcHVibGljIGFzeW5jIHBvc3Q8VFJlc291cmNlIGV4dGVuZHMgUmVzb3VyY2UsIFRFcnJvciBleHRlbmRzIFJlc291cmNlPih1cmk6IHN0cmluZywgYm9keTogdW5rbm93biwgVFJlc291cmNlOiB7IG5ldygpOiBUUmVzb3VyY2UgfSwgVEVycm9yOiB7IG5ldygpOiBURXJyb3IgfSwgaGVhZGVycz86IEh0dHBIZWFkZXJzKTogUHJvbWlzZTxIdHRwUmVzcG9uc2U8VFJlc291cmNlIHwgVEVycm9yPj4ge1xyXG4gICAgY29uc3Qgb3B0aW9ucyA9IEhhbENsaWVudC5jcmVhdGVPcHRpb25zKGhlYWRlcnMpO1xyXG4gICAgbGV0IGR0b1Jlc3BvbnNlOiBIdHRwUmVzcG9uc2U8UmVzb3VyY2VEdG8+IHwgdW5kZWZpbmVkO1xyXG4gICAgdHJ5IHtcclxuICAgICAgZHRvUmVzcG9uc2UgPSBhd2FpdCB0aGlzLl9odHRwQ2xpZW50LnBvc3Q8VFJlc291cmNlPih1cmksIGJvZHksIG9wdGlvbnMpLnRvUHJvbWlzZSgpO1xyXG4gICAgfVxyXG4gICAgY2F0Y2ggKGUpIHtcclxuICAgICAgaWYgKGUgaW5zdGFuY2VvZiBIdHRwRXJyb3JSZXNwb25zZSlcclxuICAgICAgICBkdG9SZXNwb25zZSA9IEhhbENsaWVudC5jb252ZXJ0RXJyb3JSZXNwb25zZShlKTtcclxuICAgICAgZWxzZVxyXG4gICAgICAgIHRocm93IG5ldyBFcnJvcihgUE9TVCAke3VyaX0gLSBvcHRpb25zOiAke29wdGlvbnN9IC0gYm9keTogJHtib2R5fSBmYWlsZWQgd2l0aCBlcnJvciAke2V9YCk7XHJcbiAgICB9XHJcbiAgICBpZiAoIWR0b1Jlc3BvbnNlKVxyXG4gICAgICB0aHJvdyBuZXcgRXJyb3IoYFBPU1QgJHt1cml9IC0gb3B0aW9uczogJHtvcHRpb25zfSAtIGJvZHk6ICR7Ym9keX0gZGlkIG5vdCByZXR1cm4gYSByZXNwb25zZS5gKTtcclxuICAgIGNvbnN0IHJlc291cmNlUmVzcG9uc2UgPSBIYWxDbGllbnQuY29udmVydFJlc3BvbnNlPFRSZXNvdXJjZSB8IFRFcnJvcj4oZHRvUmVzcG9uc2Uub2sgPyBUUmVzb3VyY2UgOiBURXJyb3IsIGR0b1Jlc3BvbnNlKTtcclxuICAgIHJldHVybiByZXNvdXJjZVJlc3BvbnNlO1xyXG4gIH1cclxuXHJcbiAgcHVibGljIGFzeW5jIHB1dDxUUmVzb3VyY2UgZXh0ZW5kcyBSZXNvdXJjZSwgVEVycm9yIGV4dGVuZHMgUmVzb3VyY2U+KHVyaTogc3RyaW5nLCBib2R5OiB1bmtub3duLCBUUmVzb3VyY2U6IHsgbmV3KCk6IFRSZXNvdXJjZSB9LCBURXJyb3I6IHsgbmV3KCk6IFRFcnJvciB9LCBoZWFkZXJzPzogSHR0cEhlYWRlcnMpOiBQcm9taXNlPEh0dHBSZXNwb25zZTxUUmVzb3VyY2UgfCBURXJyb3I+PiB7XHJcbiAgICBjb25zdCBvcHRpb25zID0gSGFsQ2xpZW50LmNyZWF0ZU9wdGlvbnMoaGVhZGVycyk7XHJcbiAgICBsZXQgZHRvUmVzcG9uc2U6IEh0dHBSZXNwb25zZTxSZXNvdXJjZUR0bz4gfCB1bmRlZmluZWQ7XHJcbiAgICB0cnkge1xyXG4gICAgICBkdG9SZXNwb25zZSA9IGF3YWl0IHRoaXMuX2h0dHBDbGllbnQucHV0PFRSZXNvdXJjZT4odXJpLCBib2R5LCBvcHRpb25zKS50b1Byb21pc2UoKTtcclxuICAgIH1cclxuICAgIGNhdGNoIChlKSB7XHJcbiAgICAgIGlmIChlIGluc3RhbmNlb2YgSHR0cEVycm9yUmVzcG9uc2UpXHJcbiAgICAgICAgZHRvUmVzcG9uc2UgPSBIYWxDbGllbnQuY29udmVydEVycm9yUmVzcG9uc2UoZSk7XHJcbiAgICAgIGVsc2VcclxuICAgICAgICB0aHJvdyBuZXcgRXJyb3IoYFBVVCAke3VyaX0gLSBvcHRpb25zOiAke29wdGlvbnN9IC0gYm9keTogJHtib2R5fSBmYWlsZWQgd2l0aCBlcnJvciAke2V9YCk7XHJcbiAgICB9XHJcbiAgICBpZiAoIWR0b1Jlc3BvbnNlKVxyXG4gICAgICB0aHJvdyBuZXcgRXJyb3IoYFBVVCAke3VyaX0gLSBvcHRpb25zOiAke29wdGlvbnN9IC0gYm9keTogJHtib2R5fSBkaWQgbm90IHJldHVybiBhIHJlc3BvbnNlLmApO1xyXG4gICAgY29uc3QgcmVzb3VyY2VSZXNwb25zZSA9IEhhbENsaWVudC5jb252ZXJ0UmVzcG9uc2U8VFJlc291cmNlIHwgVEVycm9yPihkdG9SZXNwb25zZS5vayA/IFRSZXNvdXJjZSA6IFRFcnJvciwgZHRvUmVzcG9uc2UpO1xyXG4gICAgcmV0dXJuIHJlc291cmNlUmVzcG9uc2U7XHJcbiAgfVxyXG5cclxuICBwdWJsaWMgYXN5bmMgZGVsZXRlPFRFcnJvciBleHRlbmRzIFJlc291cmNlPih1cmk6IHN0cmluZywgVEVycm9yOiB7IG5ldygpOiBURXJyb3IgfSwgaGVhZGVycz86IEh0dHBIZWFkZXJzKTogUHJvbWlzZTxIdHRwUmVzcG9uc2U8dm9pZCB8IFRFcnJvcj4+IHtcclxuICAgIGNvbnN0IG9wdGlvbnMgPSBIYWxDbGllbnQuY3JlYXRlT3B0aW9ucyhoZWFkZXJzKTtcclxuICAgIGxldCByZXNwb25zZTogSHR0cFJlc3BvbnNlPFJlc291cmNlRHRvIHwgdm9pZD4gfCB1bmRlZmluZWQ7XHJcbiAgICB0cnkge1xyXG4gICAgICByZXNwb25zZSA9IGF3YWl0IHRoaXMuX2h0dHBDbGllbnQuZGVsZXRlPHZvaWQ+KHVyaSwgb3B0aW9ucykudG9Qcm9taXNlKCk7XHJcbiAgICB9XHJcbiAgICBjYXRjaCAoZSkge1xyXG4gICAgICBpZiAoZSBpbnN0YW5jZW9mIEh0dHBFcnJvclJlc3BvbnNlKVxyXG4gICAgICAgIHJlc3BvbnNlID0gSGFsQ2xpZW50LmNvbnZlcnRFcnJvclJlc3BvbnNlKGUpO1xyXG4gICAgICBlbHNlXHJcbiAgICAgICAgdGhyb3cgbmV3IEVycm9yKGBERUxFVEUgJHt1cml9IC0gb3B0aW9uczogJHtvcHRpb25zfSBmYWlsZWQgd2l0aCBlcnJvciAke2V9YCk7XHJcbiAgICB9XHJcbiAgICBpZiAoIXJlc3BvbnNlKVxyXG4gICAgICB0aHJvdyBuZXcgRXJyb3IoYERFTEVURSAke3VyaX0gLSBvcHRpb25zOiAke29wdGlvbnN9IGRpZCBub3QgcmV0dXJuIGEgcmVzcG9uc2UuYCk7XHJcbiAgICBpZiAoIXJlc3BvbnNlLm9rKSB7XHJcbiAgICAgIGNvbnN0IGVycm9yUmVzcG9uc2UgPSBIYWxDbGllbnQuY29udmVydFJlc3BvbnNlPFRFcnJvcj4oVEVycm9yLCByZXNwb25zZSBhcyBIdHRwUmVzcG9uc2U8UmVzb3VyY2VEdG8+KTtcclxuICAgICAgcmV0dXJuIGVycm9yUmVzcG9uc2U7XHJcbiAgICB9XHJcblxyXG4gICAgcmV0dXJuIHJlc3BvbnNlIGFzIEh0dHBSZXNwb25zZTx2b2lkPjtcclxuICB9XHJcblxyXG4gIHByaXZhdGUgc3RhdGljIGNyZWF0ZU9wdGlvbnMoaGVhZGVycz86IEh0dHBIZWFkZXJzKTogeyBoZWFkZXJzPzogSHR0cEhlYWRlcnM7IHJlc3BvbnNlVHlwZTogJ2pzb24nOyBvYnNlcnZlOiAncmVzcG9uc2UnIH0ge1xyXG4gICAgaGVhZGVycz8uYXBwZW5kKCdBY2NlcHQnLCAnYXBwbGljYXRpb24vaGFsK2pzb24nKVxyXG4gICAgcmV0dXJuIHtcclxuICAgICAgaGVhZGVyczogaGVhZGVycyxcclxuICAgICAgcmVzcG9uc2VUeXBlOiAnanNvbicsXHJcbiAgICAgIG9ic2VydmU6ICdyZXNwb25zZSdcclxuICAgIH1cclxuICB9XHJcblxyXG4gIHB1YmxpYyBzdGF0aWMgY29udmVydFJlc3BvbnNlPFRSZXNvdXJjZSBleHRlbmRzIFJlc291cmNlPihUUmVzb3VyY2U6IHsgbmV3KGR0bz86IFJlc291cmNlRHRvIHwgbnVsbCk6IFRSZXNvdXJjZSB9LCByZXNwb25zZTogSHR0cFJlc3BvbnNlPFJlc291cmNlRHRvPik6IEh0dHBSZXNwb25zZTxUUmVzb3VyY2U+IHtcclxuICAgIGNvbnN0IHJlc291cmNlID0gbmV3IFRSZXNvdXJjZShyZXNwb25zZS5ib2R5KTtcclxuICAgIGNvbnN0IHJlc291cmNlUmVzcG9uc2UgPSByZXNwb25zZS5jbG9uZTxUUmVzb3VyY2U+KHsgYm9keTogcmVzb3VyY2UgfSk7XHJcbiAgICByZXR1cm4gcmVzb3VyY2VSZXNwb25zZTtcclxuICB9XHJcblxyXG4gIHByaXZhdGUgc3RhdGljIGNvbnZlcnRFcnJvclJlc3BvbnNlPFRSZXNvdXJjZUR0bz4oZTogSHR0cEVycm9yUmVzcG9uc2UpOiBIdHRwUmVzcG9uc2U8VFJlc291cmNlRHRvPiB7XHJcbiAgICBjb25zdCBkdG9SZXNwb25zZSA9IG5ldyBIdHRwUmVzcG9uc2UoeyBib2R5OiBlLmVycm9yLCBoZWFkZXJzOiBlLmhlYWRlcnMsIHN0YXR1czogZS5zdGF0dXMsIHN0YXR1c1RleHQ6IGUuc3RhdHVzVGV4dCwgdXJsOiBlLnVybCA/IGUudXJsIDogdW5kZWZpbmVkIH0pO1xyXG4gICAgcmV0dXJuIGR0b1Jlc3BvbnNlO1xyXG4gIH1cclxufVxyXG4iXX0=