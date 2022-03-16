import { sleep } from 'k6';
import http from 'k6/http';

export const options = {
    vus: 100,
    duration: '30s',
    thresholds: {
      'http_req_duration{status:200}': ['max>=0'],
      'http_req_duration{status:500}': ['max>=0'],
    },
    'summaryTrendStats': ['min', 'med', 'avg', 'p(90)', 'p(95)', 'max', 'count'],
  };

export default function () {
    const baseUrl = 'https://localhost:7212/';
    http.get(`${baseUrl}api/entity-pump`);
    sleep(1);
}