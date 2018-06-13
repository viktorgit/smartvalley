import {HttpClient, HttpParams} from '@angular/common/http';
import {BaseApiClient} from '../base-api-client';
import {Injectable} from '@angular/core';
import {AllotmentEventResponse} from './responses/allotment-event-response';
import {CollectionResponse} from '../collection-response';
import {GetAllotmentEventsRequest} from './request/get-allotment-events-request';
import {CreateAllotmentEventRequest} from './request/create-allotment-event-request';
import {CreateAllotmentEventResponse} from './responses/create-allotment-event-response';

@Injectable()
export class AllotmentEventsApiClient extends BaseApiClient {
  constructor(private http: HttpClient) {
    super();
  }

  public async getAllotmentEvents(params: GetAllotmentEventsRequest): Promise<CollectionResponse<AllotmentEventResponse>> {

    let parameters = new HttpParams()
      .append('offset', params.offset.toString())
      .append('count', params.count.toString());

    if (params.status) {
      params.status.forEach(status => {
        parameters = parameters.append('allotmentEventStatuses', status.toString());
      });
    }

    return this.http.get<CollectionResponse<AllotmentEventResponse>>(`${this.baseApiUrl}/allotmentEvents/`, {
      params: parameters
    }).toPromise();
  }

  public createAsync(name: string,
                     tokenContractAddress: string,
                     tokenDecimals: number,
                     tokenTicker: string,
                     projectId: number,
                     finishDate?: Date): Promise<CreateAllotmentEventResponse> {
    return this.http.post<CreateAllotmentEventResponse>(`${this.baseApiUrl}/allotmentEvents/`, <CreateAllotmentEventRequest>{
      name: name,
      tokenContractAddress: tokenContractAddress,
      tokenDecimals: tokenDecimals,
      tokenTicker: tokenTicker,
      projectId: projectId,
      finishDate: finishDate
    }).toPromise();
  }

  public publishAsync(eventId: number, transactionHash: string) {
    return this.http.put(`${this.baseApiUrl}/allotmentEvents/${eventId}/publish/`, {transactionHash: transactionHash}).toPromise();
  }
}
