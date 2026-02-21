INSERT 
INTO application_configs( 
  nippo_stop_date
  , ms_tenant_id
  , ms_client_id
  , ms_client_secret
  , smtp_user
  , smtp_password
) 
VALUES ( 
  '2026/10/01'
  , 'ms_tenant_id'
  , 'ms_client_id'
  , 'ms_client_secret'
  , 'smtp_user'
  , 'smtp_password'
);


INSERT 
INTO busyos( 
  code
  , name
  , kananame
  , oyacode
  , startymd
  , endymd
  , jyunjyo
  , kasyocode
  , kaikeicode
  , keiricode
  , activeflag
  , ryakusyou
  , busyobaseid
  , oyaid
) 
VALUES ( 
  '131'
  , ''
  , ''
  , null
  , '20241001'
  , '20991231'
  , 1
  , '20'
  , '131'
  , 'SH'
  , true
  , ''
  , 1
  , null
);

INSERT 
INTO syain_bases(name, code) 
VALUES ('赤岩 大輔', '01189');


INSERT 
INTO syains( 
  code
  , name
  , kananame
  , seibetsu
  , busyocode
  , syokusyucode
  , syokusyubunruicode
  , nyuusyaymd
  , startymd
  , endymd
  , kyusyoku
  , syucyosyokui
  , kingssyozoku
  , kaisyacode
  , kintaizokusei
  , genkarendouflag
  , email
  , keitaimail
  , kengen
  , jyunjyo
  , taisyoku
  , userrolecode
  , phone_number
  , syainbaseid
  , busyoid
  , kintaizokusei_id
) 
VALUES ( 
  '01189'
  , '赤岩 大輔'
  , 'アカイワ ダイスケ'
  , 1
  , '131'
  , '10'
  , 1
  , '20241001'
  , '20241001'
  , '20991231'
  , '7'
  , 1
  , '131'
  , '100'
  , 1
  , true
  , 'akaiwa@star.kyotec.co.jp'
  , 'akaiwa@star.kyotec.co.jp'
  , 1
  , 1
  , false
  , 1
  , ''
  , 1 -- syainbaseid
  , 2 -- busyoid
  , 1
);

