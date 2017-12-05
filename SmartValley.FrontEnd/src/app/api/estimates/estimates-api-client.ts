import {HttpClient, HttpParams} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {BaseApiClient} from '../base-api-client';
import {SubmitEstimatesRequest} from './submit-estimates-request';
import {CollectionResponse} from '../collection-response';
import {EstimateResponse} from './estimate-response';
import {ExpertiseArea} from '../scoring/expertise-area.enum';
import {GetEstimatesResponse} from './get-estimates-response';

@Injectable()
export class EstimatesApiClient extends BaseApiClient {
  constructor(private http: HttpClient) {
    super();
  }

  async submitEstimatesAsync(request: SubmitEstimatesRequest): Promise<void> {
    await this.http.post(this.baseApiUrl + '/estimates', request).toPromise();
  }

  async getByProjectIdAndExpertiseAreaAsync(projectId: number, expertiseArea: ExpertiseArea): Promise<GetEstimatesResponse> {
    const parameters = new HttpParams()
      .append('projectId', projectId.toString())
      .append('expertiseArea', expertiseArea.toString());

    return this.http
      .get<GetEstimatesResponse>(this.baseApiUrl + '/estimates', {params: parameters})
      .toPromise();
  }
}
