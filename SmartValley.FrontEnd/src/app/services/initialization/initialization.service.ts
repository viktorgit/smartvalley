import {Injectable} from '@angular/core';
import {ScoringCriterionService} from '../criteria/scoring-criterion.service';
import {PromiseUtils} from '../../utils/promise-utils';
import {ScoringManagerContractClient} from '../contract-clients/scoring-manager-contract-client';
import {AuthenticationService} from '../authentication/authentication-service';
import {AdminContractClient} from '../contract-clients/admin-contract-client';
import {ExpertsRegistryContractClient} from '../contract-clients/experts-registry-contract-client';
import {AreaService} from '../expert/area.service';
import {ScoringExpertsManagerContractClient} from '../contract-clients/scoring-experts-manager-contract-client';
import {DictionariesService} from '../common/dictionaries.service';

@Injectable()
export class InitializationService {

  public isAppInitialized: boolean;

  constructor(private scoringCriterionService: ScoringCriterionService,
              private adminContractClient: AdminContractClient,
              private expertContractClient: ExpertsRegistryContractClient,
              private scoringManagerContractClient: ScoringManagerContractClient,
              private scoringExpertsManagerContractClient: ScoringExpertsManagerContractClient,
              private authenticationService: AuthenticationService,
              private areaService: AreaService,
              private dictionariesService: DictionariesService) {
  }

  public async initializeAppAsync(): Promise<void> {
    if (this.isAppInitialized) {
      return;
    }

    await Promise.all([this.initializeAppInternalAsync(), this.waitAsync()]);
    this.isAppInitialized = true;
  }

  private async initializeAppInternalAsync(): Promise<void> {
    await Promise.all([
      this.authenticationService.initializeAsync(),
      this.scoringCriterionService.initializeAsync(),
      this.adminContractClient.initializeAsync(),
      this.scoringManagerContractClient.initializeAsync(),
      this.scoringExpertsManagerContractClient.initializeAsync(),
      this.expertContractClient.initializeAsync(),
      this.areaService.initializeAsync(),
      this.dictionariesService.initializeAsync()
    ]);
  }

  private async waitAsync(): Promise<void> {
    await PromiseUtils.delay(1 * 1000);
  }
}
