import http from 'k6/http';
import { sleep } from 'k6';

export let options = {
  vus: 199999,
  duration: '30s',
};

export default function () {
  http.get('https://azurecourse2021publicapi.azurewebsites.net');
  sleep(1);
}