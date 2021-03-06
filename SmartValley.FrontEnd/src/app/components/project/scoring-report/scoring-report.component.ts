import {Component, Inject, Input, OnInit} from '@angular/core';
import {AreaService} from '../../../services/expert/area.service';
import {EstimatesApiClient} from '../../../api/estimates/estimates-api-client';
import {AreasScoringInfo} from './areas-scoring-Info';
import {ScoringCriteriaGroup} from '../../../services/criteria/scoring-criteria-group';
import {ScoringCriterionService} from '../../../services/criteria/scoring-criterion.service';
import {Paths} from '../../../paths';
import {Router} from '@angular/router';
import {ScoringStatus} from '../../../services/scoring-status.enum';
import {OfferStatus} from '../../../api/scoring-offer/offer-status.enum';
import {AreaType} from '../../../api/scoring/area-type.enum';
import {CriterionPromptResponse} from '../../../api/estimates/responses/criterion-prompt-response';
import {CriterionPrompt} from '../../../api/estimates/criterion-prompt';
import {QuestionControlType} from '../../../api/scoring-application/question-control-type.enum';
import {ProjectComponent} from '../project.component';
import {UserContext} from '../../../services/authentication/user-context';
import {ExpertResponse} from '../../../api/expert/expert-response';
import {ScoringCriterionOption} from '../../../services/criteria/scoring-criterion-option';

@Component({
  selector: 'app-scoring-report',
  templateUrl: './scoring-report.component.html',
  styleUrls: ['./scoring-report.component.scss']
})
export class ScoringReportComponent implements OnInit {

  public areasScoringInfo: AreasScoringInfo[] = [];
  public questionTypeComboBox = QuestionControlType.Combobox;
  public scoringCriterionResponse: ScoringCriteriaGroup[] = [];
  public criterionPrompts: CriterionPromptResponse[] = [];
  public questionsActivity: boolean[] = [];
  public areaType: number;
  public criterionIsReady = false;
  public isAdmin = false;
  public experts: ExpertResponse[] = [];
  public hintsArray: { id: number; options: ScoringCriterionOption[]}[] = [];

  public ScoringStatus = ScoringStatus;

  @Input() projectId: number;
  @Input() isPrivate: boolean;
  @Input() scoringStatus: ScoringStatus;
  @Input() isAuthor: boolean;

  constructor(@Inject(ProjectComponent) public parent: ProjectComponent,
              private router: Router,
              private userContext: UserContext,
              private areaService: AreaService,
              private estimatesApiClient: EstimatesApiClient,
              private scoringCriterionService: ScoringCriterionService) {
  }

  public async ngOnInit() {
    const currentUser = this.userContext.getCurrentUser();
    if (currentUser) {
      this.isAdmin = currentUser.isAdmin;
    }
    const estimates = await this.estimatesApiClient.getAsync(this.projectId);
    if (this.isAdmin) {
      this.experts = estimates.experts;
    }
    for (const item of estimates.scoringReportsInArea) {
      const estimatesForArea = estimates.scoringReportsInArea.find(e => e.areaType === item.areaType);
      this.areasScoringInfo.push({
        finishedExperts: estimatesForArea.offers.filter(o => o.status === OfferStatus.Finished).length,
        totalExperts: estimatesForArea.requiredExpertsCount,
        areaName: this.areaService.getNameByType(item.areaType),
        scoringInfo: estimatesForArea,
        areaType: item.areaType
      });
      this.scoringCriterionResponse = this.scoringCriterionResponse.concat(this.scoringCriterionService.getByArea(item.areaType));
      const criterionPromptsResponse = await this.estimatesApiClient.getCriterionPromptsAsync(this.projectId, item.areaType);
      this.criterionPrompts = this.criterionPrompts.concat(criterionPromptsResponse.items);
    }
    const hints = this.scoringCriterionResponse.map( s => s.criteria.map(c => {
      return [{
        id: c.id,
        options: c.options
      }];
    }).reduce((l, r) => l.concat(r)));
    this.hintsArray = hints.reduce( (l, r) => l.concat(r));
    this.criterionIsReady = true;
    this.questionsActivity = [true];
  }

  public getCriterionInfo(id): CriterionPrompt[] {
    const criterionPrompts = this.criterionPrompts.find((c) => c.scoringCriterionId === id);
    if (criterionPrompts) {
      return criterionPrompts.prompts;
    }
    return null;
  }

  public getHintOptions(id: number): ScoringCriterionOption[] {
    return this.hintsArray.find(h => h.id === id).options;
  }

  public getExpertById(expertId: number) {
    return this.experts.firstOrDefault(i => i.id === expertId);
  }

  public getMaxScoreByArea(areaType: AreaType): number {
    return this.areaService.getMaxScore(+areaType);
  }

  public getQuestionById(id: number): string {
    return this.scoringCriterionResponse.selectMany(s => s.criteria).find(c => c.id === id).name;
  }

  public async navigateToApplicationScoringAsync(): Promise<void> {
    await this.router.navigate(['/' + Paths.ScoringApplication + '/' + this.projectId]);
  }

  public getFinishedExperts(areaType: number): number {
    return this.areasScoringInfo.filter(a => a.areaType === areaType)
      .selectMany(e => e.scoringInfo.offers)
      .filter(o => o.status === OfferStatus.Finished)
      .length;
  }

  public changeActiveQuestion(id: number) {
    const indexOfCurrentSelected = this.questionsActivity.indexOf(true);
    this.questionsActivity = this.questionsActivity.map(() => false);
    if (indexOfCurrentSelected !== id) {
      this.questionsActivity[id] = !this.questionsActivity[id];
    }
  }

  public onTabChange() {
    this.questionsActivity = [];
  }
}
