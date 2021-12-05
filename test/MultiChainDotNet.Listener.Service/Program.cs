using Microsoft.AspNetCore.Http.Json;
using MultiChainDotNet.Core.MultiChainTransaction;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);
var repo = new RawTransactionsRepository("multichain-listener");

builder.Services.AddControllers()
	.AddNewtonsoftJson();

//builder.Services.Configure<JsonOptions>(options =>
//{
//});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
	options.AddPolicy(name: "localhostOnly",
		builder =>
		{
			builder.WithOrigins("http://localhost");
		});
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.MapControllers();

app.MapGet("/RawTransactions", async () =>
	{
		var result = await repo.ListAll();
		return result;
	})
	.WithName("GetRawTransactions");

app.MapPost("/RawTransactions", async (DecodeRawTransactionResult content) =>
	{
		await repo.Insert(content);
	})
	.WithName("AddRawTransactions");

app.MapDelete("/RawTransactions", async () =>
	{
		await repo.DeleteAll();
	}).WithName("DeleteRawTransactions");

app.MapGet("/GetSimpleClass", () =>
	{
		return new SimpleClass { Name = "Hello,world" };
	}).WithName("GetSimpleClass");

app.MapGet("/GetComplexClass", () =>
{
	var raw = "{'txid':'da5bcd5656eac0ef54cbf08dcff83aeb8929675556f0442e6835efc16ae304bf','version':1,'locktime':0,'vin':[{'txid':'0c9c28d671e8f2fb2a4c347cdff4033ad8f2f8b505924c18d9bbeb9e31e03d24','vout':0,'scriptSig':{'asm':'0 30440220588dbdd01f6769eca130fbcc5504878c95b3dc6f94b7d55c44d2239758df1a79022010420a9f64c2055b7baeb3ee32e8eaa402b1db1019e4bd92df160133c55ca31501 304402201d0ed6038f1b38ea71b747ac25ff3fecceaafffc0059aba9dc211225072211a602201835ad908eb3515385ca86d9c5cfeaf0aee040a937458fdfb94ada106d88622c01 522103013ffb59769ea760da19bcc6a22bcb7b0e4a4a1ff64e862916af2703758b8fa02103cbb355bd0f558b892113dff5f45c847ef948219673f784970beb5fc532effe8021039b3e46c0d89ae930bcc8d7b45c7e149d14bd69e45cfaec84baf328779d38136153ae','hex':'004730440220588dbdd01f6769eca130fbcc5504878c95b3dc6f94b7d55c44d2239758df1a79022010420a9f64c2055b7baeb3ee32e8eaa402b1db1019e4bd92df160133c55ca3150147304402201d0ed6038f1b38ea71b747ac25ff3fecceaafffc0059aba9dc211225072211a602201835ad908eb3515385ca86d9c5cfeaf0aee040a937458fdfb94ada106d88622c014c69522103013ffb59769ea760da19bcc6a22bcb7b0e4a4a1ff64e862916af2703758b8fa02103cbb355bd0f558b892113dff5f45c847ef948219673f784970beb5fc532effe8021039b3e46c0d89ae930bcc8d7b45c7e149d14bd69e45cfaec84baf328779d38136153ae'},'sequence':4294967295}],'vout':[{'value':0,'n':0,'scriptPubKey':{'asm':'OP_DUP OP_HASH160 b33ef123e5caf001824ca1c0c7214ca903a1e7dc OP_EQUALVERIFY OP_CHECKSIG 73706b6f14b13b806ee6876fed5065d68f1563e3e803000000000000 OP_DROP','hex':'76a914b33ef123e5caf001824ca1c0c7214ca903a1e7dc88ac1c73706b6f14b13b806ee6876fed5065d68f1563e3e80300000000000075','reqSigs':1,'type':'pubkeyhash','addresses':['1RE72u8HPMBWwYFDLyjoNJHUQwyPkpwk3fc2QN']},'assets':[{'name':'AQEBAHm1G97+3O9MHSvG3Zs3kdkiTT6Q','issuetxid':'e363158fd66550ed6f87e66e803bb114c712911415358f0b24f88baf68961b51','assetref':'60-4193-25571','qty':1000,'raw':1000,'type':'issuemore'}]},{'value':0,'n':1,'scriptPubKey':{'asm':'73706b6602 OP_DROP OP_RETURN 7b690c52656c61794d6573736167657b690249645369403331343433376138353264633132383038613562366437386466323131623563643137623135633631643136356236623038643961623635316466303038373169094d6573736167654964536940333134343337613835326463313238303861356236643738646632313162356364313762313563363164313635623662303864396162363531646630303837316917526563697069656e745472616e736974416464726573735369263152453732753848504d4257775946444c796a6f4e4a4855517779506b70776b33666332514e6907417373657449645369204151454241486d314739372b334f394d48537647335a73336b646b6954543651690841737365745174794903e8690d5061636b6167654865616465727b690249645369403331343433376138353264633132383038613562366437386466323131623563643137623135633631643136356236623038643961623635316466303038373169095061636b616765496453694033313434333761383532646331323830386135623664373864663231316235636431376231356336316431363562366230386439616236353164663030383731690c4f726967696e61746f7249647b6902496453694030313030303131346363616536613134656538636431656561323735313837613965353733363064313662646630633230303030303030303030303030303030690756657273696f6e690169094e6574776f726b4964690169064c656e677468691469074164647265737353691c7a4b3571464f364d30653669645268366e6c633244526139384d493d7d690b526563697069656e7449647b6902496453694030313030303031633030623333656631323365353064636166303031383234633566613163306337323134636563613930336131653764633632633432613437690756657273696f6e690169094e6574776f726b4964690069064c656e677468691c690741646472657373536928414c4d2b3853506c4463727741594a4d583648417879464d374b6b446f6566635973517152773d3d7d6907546f6b656e49647b6902496453694030313030303131343739623531626465666564636566346331643262633664643962333739316439323234643365393030303030303030303030303030303030690756657273696f6e690169094e6574776f726b4964690169064c656e677468691469074164647265737353691c6562556233763763373077644b3862646d7a655232534a4e5070413d7d690d5061636b616765547970654964690169145061636b61676548656164657256657273696f6e6901690c5061636b6167654e6f6e63654c08d9ab651df008716904446174617b69064c656e67746869127d7d69075061636b6167655355d841514142464d7975616854756a4e48756f6e5559657035584e6730577666444341414141414141414141414241414163414c4d2b3853506c4463727741594a4d583648417879464d374b6b446f656663597351715277454141525235745276652f747a7654423072787432624e35485a496b302b6b4141414141414141414141415145493261746c486641496359653231453441414141414141414141414141414141414141414141414141414141414141414141414141414141414141414141414141414141414141414e344c617a7032514141413d3d6909446972656374696f6e69016905537461746569007d7d','hex':'0573706b6602756a4dfd047b690c52656c61794d6573736167657b690249645369403331343433376138353264633132383038613562366437386466323131623563643137623135633631643136356236623038643961623635316466303038373169094d6573736167654964536940333134343337613835326463313238303861356236643738646632313162356364313762313563363164313635623662303864396162363531646630303837316917526563697069656e745472616e736974416464726573735369263152453732753848504d4257775946444c796a6f4e4a4855517779506b70776b33666332514e6907417373657449645369204151454241486d314739372b334f394d48537647335a73336b646b6954543651690841737365745174794903e8690d5061636b6167654865616465727b690249645369403331343433376138353264633132383038613562366437386466323131623563643137623135633631643136356236623038643961623635316466303038373169095061636b616765496453694033313434333761383532646331323830386135623664373864663231316235636431376231356336316431363562366230386439616236353164663030383731690c4f726967696e61746f7249647b6902496453694030313030303131346363616536613134656538636431656561323735313837613965353733363064313662646630633230303030303030303030303030303030690756657273696f6e690169094e6574776f726b4964690169064c656e677468691469074164647265737353691c7a4b3571464f364d30653669645268366e6c633244526139384d493d7d690b526563697069656e7449647b6902496453694030313030303031633030623333656631323365353064636166303031383234633566613163306337323134636563613930336131653764633632633432613437690756657273696f6e690169094e6574776f726b4964690069064c656e677468691c690741646472657373536928414c4d2b3853506c4463727741594a4d583648417879464d374b6b446f6566635973517152773d3d7d6907546f6b656e49647b6902496453694030313030303131343739623531626465666564636566346331643262633664643962333739316439323234643365393030303030303030303030303030303030690756657273696f6e690169094e6574776f726b4964690169064c656e677468691469074164647265737353691c6562556233763763373077644b3862646d7a655232534a4e5070413d7d690d5061636b616765547970654964690169145061636b61676548656164657256657273696f6e6901690c5061636b6167654e6f6e63654c08d9ab651df008716904446174617b69064c656e67746869127d7d69075061636b6167655355d841514142464d7975616854756a4e48756f6e5559657035584e6730577666444341414141414141414141414241414163414c4d2b3853506c4463727741594a4d583648417879464d374b6b446f656663597351715277454141525235745276652f747a7654423072787432624e35485a496b302b6b4141414141414141414141415145493261746c486641496359653231453441414141414141414141414141414141414141414141414141414141414141414141414141414141414141414141414141414141414141414e344c617a7032514141413d3d6909446972656374696f6e69016905537461746569007d7d','type':'nulldata'},'data':[{'json':{'RelayMessage':{'Id':'314437a852dc12808a5b6d78df211b5cd17b15c61d165b6b08d9ab651df00871','MessageId':'314437a852dc12808a5b6d78df211b5cd17b15c61d165b6b08d9ab651df00871','RecipientTransitAddress':'1RE72u8HPMBWwYFDLyjoNJHUQwyPkpwk3fc2QN','AssetId':'AQEBAHm1G97+3O9MHSvG3Zs3kdkiTT6Q','AssetQty':1000,'PackageHeader':{'Id':'314437a852dc12808a5b6d78df211b5cd17b15c61d165b6b08d9ab651df00871','PackageId':'314437a852dc12808a5b6d78df211b5cd17b15c61d165b6b08d9ab651df00871','OriginatorId':{'Id':'01000114ccae6a14ee8cd1eea275187a9e57360d16bdf0c20000000000000000','Version':1,'NetworkId':1,'Length':20,'Address':'zK5qFO6M0e6idRh6nlc2DRa98MI='},'RecipientId':{'Id':'0100001c00b33ef123e50dcaf001824c5fa1c0c7214ceca903a1e7dc62c42a47','Version':1,'NetworkId':0,'Length':28,'Address':'ALM+8SPlDcrwAYJMX6HAxyFM7KkDoefcYsQqRw=='},'TokenId':{'Id':'0100011479b51bdefedcef4c1d2bc6dd9b3791d9224d3e900000000000000000','Version':1,'NetworkId':1,'Length':20,'Address':'ebUb3v7c70wdK8bdmzeR2SJNPpA='},'PackageTypeId':1,'PackageHeaderVersion':1,'PackageNonce':637729273031952497,'Data':{'Length':18}},'Package':'AQABFMyuahTujNHuonUYep5XNg0WvfDCAAAAAAAAAAABAAAcALM+8SPlDcrwAYJMX6HAxyFM7KkDoefcYsQqRwEAARR5tRve/tzvTB0rxt2bN5HZIk0+kAAAAAAAAAAAAQEI2atlHfAIcYe21E4AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAN4Lazp2QAAA==','Direction':1,'State':0}}}]},{'value':0,'n':2,'scriptPubKey':{'asm':'OP_HASH160 a022b7140b5f0834c2a5c3f71937097f725de9dc OP_EQUAL','hex':'a914a022b7140b5f0834c2a5c3f71937097f725de9dc87','reqSigs':1,'type':'scripthash','addresses':['4MeBuxZQjxxASWqeDjqnDw8tBnoQCpxiWKfxH9']}}],'issue':{},'blockhash':'0016f91909bac972b695d0459ab20771122407ced79209cb3bc52549dbb039af','confirmations':0}";
	var escaped = raw.Replace("'", "\"");
	var result =  JsonConvert.DeserializeObject<DecodeRawTransactionResult>(escaped);

	return result;

	//var deserialised = System.Text.Json.JsonSerializer.Deserialize<DecodeRawTransactionResult>(escaped, new System.Text.Json.JsonSerializerOptions
	//{
	//	PropertyNameCaseInsensitive = true
	//});
	//return deserialised;
}).WithName("GetComplexClass");

app.Run();