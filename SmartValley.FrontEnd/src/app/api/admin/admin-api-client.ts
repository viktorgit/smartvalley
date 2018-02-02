import {HttpClient} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {BaseApiClient} from '../base-api-client';
import {AdminRequest} from './admin-request';
import {CollectionResponse} from '../collection-response';

@Injectable()
export class AdminApiClient extends BaseApiClient {
  constructor(private http: HttpClient) {
    super();
  }

  public async addAdminAsync(address: string): Promise<void> {
    await this.http.post(this.baseApiUrl + '/admin', <AdminRequest> { address: address}).toPromise();
  }

  public async deleteAdminAsync(address: string): Promise<void> {
    await this.http.delete(this.baseApiUrl + '/admin/' + address).toPromise();
  }

  public isAdminAsync(address: string): Promise<boolean> {
    return this.http.post<boolean>(this.baseApiUrl + '/admin/isAdmin', <AdminRequest> { address: address}).toPromise();
  }

  public getAllAdminsAsync(): Promise<any> {
    return this.http.get<CollectionResponse<AdminRequest>> (this.baseApiUrl + '/admin').toPromise();
  }
}
