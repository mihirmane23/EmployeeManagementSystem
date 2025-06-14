import { Employee } from './employee';

export interface ApiResponse {
  data: Employee[];
  totalEntries: number;
}