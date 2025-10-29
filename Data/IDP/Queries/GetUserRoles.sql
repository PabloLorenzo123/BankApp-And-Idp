SELECT
	"roles"."name"
FROM "users_roles", "roles"
WHERE 1 = 1
AND "users_roles"."user_id" = @UserId
AND "users_roles"."role_id" = "roles"."role_id";
