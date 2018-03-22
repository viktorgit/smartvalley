import {HttpClient} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {BaseApiClient} from '../base-api-client';
import {ContractResponse} from './contract-response';

@Injectable()
export class ContractApiClient extends BaseApiClient {
  constructor(private http: HttpClient) {
    super();
  }

  async getScoringManagerContractAsync(): Promise<ContractResponse> {
    return await this.http.get<ContractResponse>(this.baseApiUrl + '/contracts/scoringManager')
      .toPromise();
  }

  public async getScoringContractAsync(): Promise<ContractResponse> {
    return await this.http.get<ContractResponse>(this.baseApiUrl + '/contracts/scoring')
      .toPromise();
  }

  async getScoringExpertsManagerContractAsync(): Promise<ContractResponse> {
    return await this.http.get<ContractResponse>(this.baseApiUrl + '/contracts/scoringExpertsManager')
      .toPromise();
  }

  async getAdminRegistryContractAsync(): Promise<ContractResponse> {
    return await this.http.get<ContractResponse>(this.baseApiUrl + '/contracts/adminRegistry')
      .toPromise();
  }

  async getExpertRegistryContractAsync(): Promise<ContractResponse> {
    return await this.http.get<ContractResponse>(this.baseApiUrl + '/contracts/expertsRegistry')
      .toPromise();
  }

  async getVotingManagerContractAsync(): Promise<ContractResponse> {
    return await this.http.get<ContractResponse>(this.baseApiUrl + '/contracts/votingManager')
      .toPromise();
  }

  async getVotingContractAsync(): Promise<ContractResponse> {
    return await this.http.get<ContractResponse>(this.baseApiUrl + '/contracts/voting')
      .toPromise();
  }
}
