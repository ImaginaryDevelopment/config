<Query Kind="FSharpProgram">
  <Connection>
    <ID>282ab884-278b-4d31-a865-51e85288681f</ID>
    <Persist>true</Persist>
    <IncludeSystemObjects>true</IncludeSystemObjects>
  </Connection>
</Query>

open System
let dc = new TypedDataContext()
let isNullOrEmpty s = System.String.IsNullOrEmpty(s)
let isNotNullOrEmpty s = not <| isNullOrEmpty s

let firstOption items = Seq.tryFind (fun item -> true) items
type InviteWrapper = {Org_id:int; Email_address:string ;Ui:User_invitation;Member:Member_member}

type InviteDisplayWrapper(environment, path, invitationGuid,invitationId, orgId) = 
	member x.OrgId:int = orgId
	member x.InvitationGuid:Guid = invitationGuid
	member x.InvitationId:int = invitationId
	member x.SurveyLink = new LINQPad.Hyperlinq("http://" + environment + path + invitationGuid.ToString())
	member x.GizmoLink = new LINQPad.Hyperlinq("http://www.surveygizmo.com/s3/1114681/TEST-TPDS-DEV/?vid=" + invitationGuid.ToString())

type InviteDisplayWrapper2(environment, path, invitationGuid, invitationId, orgId) =
	inherit InviteDisplayWrapper(environment, path, invitationGuid, invitationId, orgId)
	member val RemoveSms:Hyperlinq = null with get,set
	member val EmailAddress= String.Empty with get,set
	
let anySms memberId = dc.Sms_validations.Any(fun v-> v.Member_id = memberId)

let removeSmsValidation (user:Member_member) = 
	if (user.Verified_bit_field > 0s || anySms user.Member_id) && Util.ReadLine<bool>("remove sms validation?") then
		// remove verified 
		let toRemove = dc.Sms_validations.Where(fun v -> v.Member_id = user.Member_id)
		toRemove.Dump("deleting validations")
		dc.Sms_validations.DeleteAllOnSubmit(toRemove)
		user.Verified_bit_field <- 0s
		dc.SubmitChanges()
		"Sms validation should show, catcha should show".Dump()
	else
		"Sms validation should show, catcha should show".Dump()
		
	dc.Sms_validations.Where(fun v -> v.Member_id = user.Member_id).DumpIf((fun s->s.Any()),"should be empty") |> ignore
	
let filterMembers(members:IQueryable<Member_member>):IQueryable<Member_member> =
	members.Where( fun m -> 
		m.Org_id = 1 
				&& not m.Is_fraud 
				&& m.Is_active
				// && m.Add_dt > DateTime(2014,1,1)
				&& not (m.Email_address.StartsWith("trialpay")))

let environments = 
	["dev.";"qa.";"stage.";String.Empty]
	|> List.ofSeq
	|> fun e-> "localhost:16280" :: e
let path = "/SurveyInvitation.aspx?vid="
environments.Dump()

let environment = Util.ReadLine("Environment?",environments.First(),environments)
let emailAddress = Util.ReadLine("Member email? (or empty for a sample)", String.Empty, 
								filterMembers(dc.Member_members)
									.Select(fun m -> m.Email_address)
									.Take(10))
									
let q =
	query{
		for pu in dc.User_invitations do
		join m in filterMembers(dc.Member_members) on 
			(pu.Member_id = m.Member_id)
		where (pu.Prelim_survey_status_code = 'U'
			&& pu.Add_dt > DateTime(2013,12,31))
		sortByDescending pu.Add_dt
		select {InviteWrapper.Org_id = m.Org_id;Email_address= m.Email_address;Ui= pu; Member= m}
	}

if isNotNullOrEmpty emailAddress then
	emailAddress.Dump("email address selected")
	let user = 
		let mem = firstOption (q.Where(fun m -> m.Email_address = emailAddress).ToArray())
		match mem with 
		| None -> printfn "no member found for email %s" emailAddress; raise (Exception("not found"))
		| Some foundUser -> foundUser
	InviteDisplayWrapper(environment = environment,path= path, invitationGuid = user.Ui.User_invitation_guid, invitationId = user.Ui.User_invitation_id, orgId = user.Org_id).Dump("display wrapper")
	
	if (user.Member.Verified_bit_field > 0s || anySms user.Member.Member_id) && Util.ReadLine<bool>("remove sms validation?") then
		// remove verified 
		let toRemove = dc.Sms_validations.Where( fun v -> v.Member_id = user.Member.Member_id)
		toRemove.Dump("deleting validations")
		dc.Sms_validations.DeleteAllOnSubmit(toRemove)
		user.Member.Verified_bit_field <- 0s
		dc.SubmitChanges()
		"Sms validation should show, catcha should show".Dump()
	dc.Sms_validations.Where(fun v -> v.Member_id = user.Member.Member_id).DumpIf((fun s -> s.Any()),"should be empty") |> ignore
	
	if Util.ReadLine<bool>("Show result?(Take Survey before answering this)") then
		dc.User_invitations.Context.Refresh( RefreshMode.OverwriteCurrentValues,user.Ui)
		if user.Ui.Prelim_survey_status_code <> 'U' then
			user.Ui.Dump("after survey")
		else
			user.Ui.Dump("survey does not appear updated")
else
	let display = q.Take(15).ToArray()
	let removeLink mem = Hyperlinq(Action (fun () -> removeSmsValidation(mem)),"TryRemoveSms",false)
	if q.Select(fun a -> a.Email_address).Count() <> q.Select( fun a-> a.Email_address).Distinct().Count() then
		display.GroupBy( (fun s ->s.Email_address),
						fun i ->i.Member, InviteDisplayWrapper(environment = environment,path= path, invitationGuid = i.Ui.User_invitation_guid, invitationId = i.Ui.User_invitation_id, orgId = i.Org_id))
					.OrderBy( fun g -> g.Key)
					.Select(fun g ->
									let memberUser = g.First() |> (fun(m,d) -> m)
									g.Key,  removeLink memberUser, g.Select(fun (m,display) -> display).ToArray(),memberUser.Verified_bit_field, memberUser
						)
					.ToArray()
					.Dump("random")
	else
		display.Select(fun i -> 
			let removeSms = Hyperlinq(Action(fun () -> removeSmsValidation(i.Member)),"TryRemoveSms",true)
			InviteDisplayWrapper2(
				environment = environment,path= path, invitationGuid = i.Ui.User_invitation_guid, invitationId = i.Ui.User_invitation_id, orgId = i.Org_id, EmailAddress = i.Email_address,
				RemoveSms = removeSms
		)).ToArray().Dump("random")