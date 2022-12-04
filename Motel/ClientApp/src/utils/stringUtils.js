export function trim(str, ch) {
	let start = 0,
		end = str.length;

	while(start < end && str[start] === ch)
        ++start;

    while(end > start && str[end - 1] === ch)
        --end;

    return (start > 0 || end < str.length) ? str.substring(start, end) : str;
}

export const getFullname = (firstname, lastName, patronymic) =>
	(firstname ?? '')
	+ (lastName ? ' ' + lastName : '')
	+ (patronymic ? ' ' + patronymic : '');
