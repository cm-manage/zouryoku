
-- アプリConfig
-- * RestoreFromTempTable
create table application_configs (
  id bigserial not null
  , nippo_stop_date date not null
  , ms_tenant_id varchar(256) not null
  , ms_client_id varchar(256) not null
  , ms_client_secret varchar(256) not null
  , smtp_user varchar(256) not null
  , smtp_password varchar(256) not null
  , constraint application_configs_PKC primary key (id)
) ;

-- PCログ
-- * RestoreFromTempTable
create table pc_logs (
  id bigserial
  , datetime timestamp not null
  , pc_name character varying(50) not null
  , operation smallint not null
  , user_name character varying(50)
  , syain_id bigint
  , constraint pc_logs_PKC primary key (id)
) ;

-- 部署マスタ
-- * RestoreFromTempTable
create table busyos (
  id bigserial
  , code char(3) not null
  , name character varying(32)
  , kananame character varying(64)
  , oyacode char(3)
  , startymd date not null
  , endymd date
  , jyunjyo smallint
  , kasyocode char(2)
  , kaikeicode char(3)
  , keiricode char(2)
  , activeflag boolean
  , ryakusyou character varying(10)
  , busyobaseid bigint not null
  , oyaid bigint
  , constraint busyos_PKC primary key (id)
) ;

-- 社員BASEマスタ
-- * RestoreFromTempTable
create table syain_bases (
  id bigserial
  , name character varying(100)
  , code character varying(5)
  , line_token character varying(2000)
  , constraint syain_bases_PKC primary key (id)
) ;


-- 社員マスタ
-- * RestoreFromTempTable
create table syains (
  id bigserial
  , code char(5) not null
  , name character varying(32) not null
  , kananame character varying(32)
  , seibetsu char(1) not null
  , busyocode char(3)
  , syokusyucode integer
  , syokusyubunruicode integer
  , nyuusyaymd date
  , startymd date not null
  , endymd date
  , kyusyoku smallint
  , syucyosyokui smallint
  , kingssyozoku char(5)
  , kaisyacode smallint
  , kintaizokusei smallint
  , genkarendouflag boolean
  , email character varying(50)
  , keitaimail character varying(50)
  , kengen integer
  , jyunjyo smallint
  , taisyoku boolean
  , userrolecode smallint
  , phone_number character varying(15)
  , syainbaseid bigint not null
  , busyoid bigint not null
  , kintaizokusei_id bigint not null
  , constraint syains_PKC primary key (id)
) ;


alter table pc_logs
  add constraint pc_logs_FK1 foreign key (syain_id) references syains(id);

alter table syains
  add constraint syains_FK3 foreign key (busyoid) references busyos(ID);

alter table syains
  add constraint syains_FK4 foreign key (syainbaseid) references syain_bases(ID);


comment on table application_configs is 'アプリConfig';
comment on column application_configs.id is 'ID';
comment on column application_configs.nippo_stop_date is '日報停止日';
comment on column application_configs.ms_tenant_id is 'MSテナントID';
comment on column application_configs.ms_client_id is 'MSクライアントID';
comment on column application_configs.ms_client_secret is 'MSクライアントシークレット';
comment on column application_configs.smtp_password is 'SMTPパスワード';
comment on column application_configs.smtp_user is 'SMTPユーザ';


comment on table pc_logs is 'PCログ';
comment on column pc_logs.id is 'ID';
comment on column pc_logs.datetime is '日時';
comment on column pc_logs.pc_name is 'コンピューター名';
comment on column pc_logs.operation is '操作種別';
comment on column pc_logs.user_name is 'ログオンユーザ名';
comment on column pc_logs.syain_id is '社員ID';

comment on table busyos is '部署マスタ';
comment on column busyos.id is 'ID';
comment on column busyos.code is '部署番号';
comment on column busyos.name is '部署名称';
comment on column busyos.kananame is '部署カナ名称';
comment on column busyos.oyacode is '親部署番号';
comment on column busyos.startymd is '有効開始日';
comment on column busyos.endymd is '有効終了日';
comment on column busyos.jyunjyo is '並び順序';
comment on column busyos.kasyocode is '箇所コード（精算システム上の処理単位）';
comment on column busyos.kaikeicode is '会計コード（日報システム上での部署コード）';
comment on column busyos.keiricode is '経理コード（仕訳生成時の会計単位）';
comment on column busyos.activeflag is 'アクティブフラグ　0';
comment on column busyos.ryakusyou is '略称';
comment on column busyos.busyobaseid is '部署BaseID';
comment on column busyos.oyaid is '親ID';


comment on table syain_bases is '社員BASEマスタ';
comment on column syain_bases.id is 'ID';
comment on column syain_bases.name is 'NAME';
comment on column syain_bases.code is 'CODE';

comment on table syains is '社員マスタ';
comment on column syains.id is 'ID';
comment on column syains.code is '社員番号';
comment on column syains.name is '社員氏名';
comment on column syains.kananame is '社員氏名カナ';
comment on column syains.seibetsu is '性別';
comment on column syains.busyocode is '部署コード';
comment on column syains.syokusyucode is '職種コード';
comment on column syains.syokusyubunruicode is '職種分類コード';
comment on column syains.nyuusyaymd is '入社年月日';
comment on column syains.startymd is '有効開始日';
comment on column syains.endymd is '有効終了日';
comment on column syains.kyusyoku is '級職';
comment on column syains.syucyosyokui is '出張職位';
comment on column syains.kingssyozoku is 'KINGS所属';
comment on column syains.kaisyacode is '会社コード';
comment on column syains.kintaizokusei is '勤怠属性';
comment on column syains.genkarendouflag is '原価連動フラグ';
comment on column syains.email is 'Ｅ－Ｍａｉｌアドレス';
comment on column syains.keitaimail is '携帯Ｍａｉｌアドレス';
comment on column syains.kengen is '権限値';
comment on column syains.jyunjyo is '並び順序';
comment on column syains.taisyoku is '退職フラグ';
comment on column syains.userrolecode is 'USERROLECODE';
comment on column syains.phone_number is '電話番号';
comment on column syains.syainbaseid is '社員BaseID';
comment on column syains.busyoid is '部署ID';
comment on column syains.kintaizokusei_id is '勤怠属性ID';

