import { RequestError } from '@/exceptions';

export async function post(url = '', data = {}) {
  return await fetch(url, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(data)
  });
}

export async function postj(url = '', data = {}) {
  const response = await post(url, data);
  if (!response.ok) {
    console.log('Request execution error\n', response);
    if (response.headers.get('content-type')?.includes('application/problem+json'))
      throw new RequestError(response, await response.json());
	throw new RequestError(response);
  }
  return await response.json();
}

export const get = async (url = '', data = {}) => await fetch(url + '?' + new URLSearchParams(data), {method: 'GET'});

export const getj = async (url = '', data = {}) => await(await get(url, data)).json();
