import {Question} from './question';
import {TeamMemberItem} from './team-member-item';

export interface SaveScoringApplicationRequest {
  projectName: string;
  projectArea: string;
  status: string;
  projectDescription: string;
  countryCode: string;
  site: string;
  whitePaper: string;
  icoDate: string;
  contactEmail: string;
  facebookLink: string;
  bitcointalkLink: string;
  mediumLink: string;
  redditLink: string;
  telegramLink: string;
  twitterLink: string;
  gitHubLink: string;
  linkedInLink: string;
  answers: Question[];
  teamMembers: TeamMemberItem[];
  advisers: any;
}
