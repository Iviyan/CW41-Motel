import { DateTime } from "luxon";

export const isoToDate = s => DateTime.fromISO(s).toFormat("dd.MM.yyyy");
export const isoToDateTime = s => DateTime.fromISO(s).toFormat("dd.MM.yyyy HH:mm:ss");
export const secondsToDate = s => DateTime.fromMillis(s).toFormat("dd.MM.yyyy");
export const secondsToDateTime = s => DateTime.fromMillis(s).toFormat("dd.MM.yyyy HH:mm:ss");
export const secondsToInterval = s => `${ String(s / 3600 | 0).padStart(2, '0') }:${ String(s % 3600 / 60 | 0).padStart(2, '0') }:${ String(s % 60).padStart(2, '0') }`;

export const floorToDay = date => {
	date.setMilliseconds(0);
	date.setSeconds(0);
	date.setMinutes(0);
	date.setHours(0);
	return date;
}
