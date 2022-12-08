<template>
	<h3>Создание бэкапа БД</h3>
	<p>pg_dump -h 188.241.250.165 -U postgres -d motel > moteld.sql</p>
	<h3>Восстановление из бэкапа</h3>
	<p>psql -h 188.241.250.165 -U postgres -d motel &lt; moteld.sql</p>
</template>

<script>
</script>

<style scoped>

</style>
