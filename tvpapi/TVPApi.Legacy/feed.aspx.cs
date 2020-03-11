using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class feed : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Response.Clear();
        Response.Write(@"<feed>
  <export>
    <media co_guid=""ASSET_Zanskar Odyssey_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[Zanskar Odyssey]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>8/04/2012 17:23:53</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_Zanskar Odyssey_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""Zanskar Odyssey_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""Zanskar Odyssey_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_TheFallLine_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[TheFallLine]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>8/04/2012 17:23:53</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_TheFallLine_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""TheFallLine_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""TheFallLine_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_believe_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[believe]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>8/04/2012 17:23:53</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_believe_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""believe_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""believe_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_Zanskar Odyssey_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[Zanskar Odyssey]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>8/04/2012 17:23:53</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_Zanskar Odyssey_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""Zanskar Odyssey_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""Zanskar Odyssey_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_TheFallLine_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[TheFallLine]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>8/04/2012 17:23:53</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_TheFallLine_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""TheFallLine_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""TheFallLine_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_believe_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[believe]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>8/04/2012 17:23:53</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_believe_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""believe_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""believe_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_bd_Freeriders_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[bd_Freeriders]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_bd_Freeriders_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""bd_Freeriders_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""bd_Freeriders_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_bd_Storm_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[bd_Storm]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_bd_Storm_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""bd_Storm_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""bd_Storm_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_bd_Endless Winter_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[bd_Endless Winter]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_bd_Endless Winter_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""bd_Endless Winter_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""bd_Endless Winter_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_FLAKES in HD_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[FLAKES in HD]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_FLAKES in HD_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""FLAKES in HD_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""FLAKES in HD_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_BREAKING TRAIL Tour Copyp_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[BREAKING TRAIL Tour Copyp]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_BREAKING TRAIL Tour Copyp_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""BREAKING TRAIL Tour Copyp_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""BREAKING TRAIL Tour Copyp_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_Television Master 1080p_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[Television Master 1080p]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_Television Master 1080p_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""Television Master 1080p_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""Television Master 1080p_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_bd_Children_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[bd_Children]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_bd_Children_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""bd_Children_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""bd_Children_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_bd_Impact_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[bd_Impact]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_bd_Impact_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""bd_Impact_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""bd_Impact_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_bd_Off the Grid_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[bd_Off the Grid]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_bd_Off the Grid_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""bd_Off the Grid_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""bd_Off the Grid_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_bd_Journey_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[bd_Journey]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_bd_Journey_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""bd_Journey_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""bd_Journey_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_bd_Playground_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[bd_Playground]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_bd_Playground_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""bd_Playground_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""bd_Playground_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_bd_Vertical Reality_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[bd_Vertical Reality]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_bd_Vertical Reality_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""bd_Vertical Reality_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""bd_Vertical Reality_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_bd_Freeriders_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[bd_Freeriders]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_bd_Freeriders_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""bd_Freeriders_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""bd_Freeriders_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_bd_Storm_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[bd_Storm]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_bd_Storm_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""bd_Storm_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""bd_Storm_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_bd_Endless Winter_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[bd_Endless Winter]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_bd_Endless Winter_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""bd_Endless Winter_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""bd_Endless Winter_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_FLAKES in HD_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[FLAKES in HD]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_FLAKES in HD_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""FLAKES in HD_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""FLAKES in HD_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_BREAKING TRAIL Tour Copyp_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[BREAKING TRAIL Tour Copyp]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_BREAKING TRAIL Tour Copyp_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""BREAKING TRAIL Tour Copyp_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""BREAKING TRAIL Tour Copyp_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_Television Master 1080p_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[Television Master 1080p]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_Television Master 1080p_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""Television Master 1080p_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""Television Master 1080p_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_bd_Children_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[bd_Children]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_bd_Children_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""bd_Children_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""bd_Children_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_bd_Impact_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[bd_Impact]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_bd_Impact_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""bd_Impact_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""bd_Impact_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_bd_Off the Grid_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[bd_Off the Grid]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_bd_Off the Grid_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""bd_Off the Grid_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""bd_Off the Grid_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_bd_Journey_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[bd_Journey]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_bd_Journey_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""bd_Journey_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""bd_Journey_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_bd_Playground_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[bd_Playground]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_bd_Playground_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""bd_Playground_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""bd_Playground_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_bd_Vertical Reality_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[bd_Vertical Reality]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_bd_Vertical Reality_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""bd_Vertical Reality_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""bd_Vertical Reality_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_bd_Freeriders_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[bd_Freeriders]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_bd_Freeriders_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""bd_Freeriders_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""bd_Freeriders_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_bd_Storm_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[bd_Storm]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_bd_Storm_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""bd_Storm_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""bd_Storm_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_bd_Endless Winter_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[bd_Endless Winter]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_bd_Endless Winter_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""bd_Endless Winter_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""bd_Endless Winter_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_FLAKES in HD_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[FLAKES in HD]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_FLAKES in HD_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""FLAKES in HD_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""FLAKES in HD_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_BREAKING TRAIL Tour Copyp_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[BREAKING TRAIL Tour Copyp]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_BREAKING TRAIL Tour Copyp_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""BREAKING TRAIL Tour Copyp_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""BREAKING TRAIL Tour Copyp_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_Television Master 1080p_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[Television Master 1080p]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_Television Master 1080p_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""Television Master 1080p_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""Television Master 1080p_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_bd_Children_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[bd_Children]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_bd_Children_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""bd_Children_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""bd_Children_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_bd_Impact_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[bd_Impact]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_bd_Impact_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""bd_Impact_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""bd_Impact_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_bd_Off the Grid_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[bd_Off the Grid]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_bd_Off the Grid_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""bd_Off the Grid_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""bd_Off the Grid_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_bd_Journey_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[bd_Journey]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_bd_Journey_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""bd_Journey_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""bd_Journey_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_bd_Playground_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[bd_Playground]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_bd_Playground_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""bd_Playground_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""bd_Playground_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_bd_Vertical Reality_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[bd_Vertical Reality]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_bd_Vertical Reality_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""bd_Vertical Reality_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""bd_Vertical Reality_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_bd_Freeriders_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[bd_Freeriders]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_bd_Freeriders_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""bd_Freeriders_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""bd_Freeriders_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_bd_Storm_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[bd_Storm]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_bd_Storm_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""bd_Storm_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""bd_Storm_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_bd_Endless Winter_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[bd_Endless Winter]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_bd_Endless Winter_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""bd_Endless Winter_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""bd_Endless Winter_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_FLAKES in HD_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[FLAKES in HD]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_FLAKES in HD_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""FLAKES in HD_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""FLAKES in HD_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_BREAKING TRAIL Tour Copyp_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[BREAKING TRAIL Tour Copyp]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_BREAKING TRAIL Tour Copyp_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""BREAKING TRAIL Tour Copyp_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""BREAKING TRAIL Tour Copyp_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_Television Master 1080p_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[Television Master 1080p]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_Television Master 1080p_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""Television Master 1080p_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""Television Master 1080p_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_bd_Children_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[bd_Children]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_bd_Children_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""bd_Children_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""bd_Children_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_bd_Impact_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[bd_Impact]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_bd_Impact_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""bd_Impact_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""bd_Impact_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_bd_Off the Grid_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[bd_Off the Grid]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_bd_Off the Grid_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""bd_Off the Grid_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""bd_Off the Grid_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_bd_Journey_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[bd_Journey]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_bd_Journey_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""bd_Journey_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""bd_Journey_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_bd_Playground_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[bd_Playground]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_bd_Playground_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""bd_Playground_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""bd_Playground_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_bd_Vertical Reality_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[bd_Vertical Reality]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>10/04/2012 15:8:19</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_bd_Vertical Reality_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""bd_Vertical Reality_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""bd_Vertical Reality_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_PW06_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[PW06]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>14/04/2012 20:33:14</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_PW06_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""PW06_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""PW06_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_rb_soundwave_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[rb_soundwave]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>14/04/2012 20:33:14</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_rb_soundwave_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""rb_soundwave_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""rb_soundwave_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_PW07_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[PW07]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>14/04/2012 20:33:14</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_PW07_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""PW07_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""PW07_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_PW05_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[PW05]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>14/04/2012 20:33:14</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_PW05_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""PW05_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""PW05_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_MI201010110037_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[MI201010110037]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>14/04/2012 20:33:14</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_MI201010110037_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""MI201010110037_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""MI201010110037_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_THE PACT MASTER_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[THE PACT MASTER]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>14/04/2012 20:33:14</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_THE PACT MASTER_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""THE PACT MASTER_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""THE PACT MASTER_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_rb_youngjaws_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[rb_youngjaws]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>15/04/2012 14:7:33</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_rb_youngjaws_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""rb_youngjaws_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""rb_youngjaws_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_rb_sc_headingottohawaii_ep14_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[rb_sc_headingottohawaii_ep14]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>15/04/2012 14:7:33</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_rb_sc_headingottohawaii_ep14_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""rb_sc_headingottohawaii_ep14_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""rb_sc_headingottohawaii_ep14_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_rb_sc_seasonrecap_ep16_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[rb_sc_seasonrecap_ep16]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>15/04/2012 14:7:33</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_rb_sc_seasonrecap_ep16_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""rb_sc_seasonrecap_ep16_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""rb_sc_seasonrecap_ep16_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_rb_stormchaser_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[rb_stormchaser]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>15/04/2012 14:7:33</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_rb_stormchaser_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""rb_stormchaser_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""rb_stormchaser_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_rb_proximityflight_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[rb_proximityflight]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>15/04/2012 14:7:33</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_rb_proximityflight_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""rb_proximityflight_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""rb_proximityflight_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_rb_expeditionzambezi_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[rb_expeditionzambezi]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>15/04/2012 14:7:33</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_rb_expeditionzambezi_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""rb_expeditionzambezi_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""rb_expeditionzambezi_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_rb_sc_bellbeach_ep3_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[rb_sc_bellbeach_ep3]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>15/04/2012 14:7:33</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_rb_sc_bellbeach_ep3_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""rb_sc_bellbeach_ep3_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""rb_sc_bellbeach_ep3_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_rb_sc_havaiifinal_ep15_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[rb_sc_havaiifinal_ep15]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>15/04/2012 14:7:33</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_rb_sc_havaiifinal_ep15_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""rb_sc_havaiifinal_ep15_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""rb_sc_havaiifinal_ep15_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_rb_sc_searchspecial_ep13_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[rb_sc_searchspecial_ep13]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>15/04/2012 14:7:33</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_rb_sc_searchspecial_ep13_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""rb_sc_searchspecial_ep13_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""rb_sc_searchspecial_ep13_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_rb_antarctica_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[rb_antarctica]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>15/04/2012 14:7:33</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_rb_antarctica_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""rb_antarctica_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""rb_antarctica_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_rb_sc_puertorico_ep12_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[rb_sc_puertorico_ep12]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>15/04/2012 14:7:33</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_rb_sc_puertorico_ep12_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""rb_sc_puertorico_ep12_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""rb_sc_puertorico_ep12_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_rb_surfinggirlsonly_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[rb_surfinggirlsonly]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>15/04/2012 14:7:33</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_rb_surfinggirlsonly_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""rb_surfinggirlsonly_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""rb_surfinggirlsonly_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_rb_sc_australiaspecial-ep2_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[rb_sc_australiaspecial-ep2]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>15/04/2012 14:7:33</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_rb_sc_australiaspecial-ep2_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""rb_sc_australiaspecial-ep2_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""rb_sc_australiaspecial-ep2_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_rb_sc_teahupoo_ep8_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[rb_sc_teahupoo_ep8]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>15/04/2012 14:7:33</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_rb_sc_teahupoo_ep8_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""rb_sc_teahupoo_ep8_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""rb_sc_teahupoo_ep8_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_rb_mbchronicles_ep2_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[rb_mbchronicles_ep2]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>15/04/2012 22:9:45</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_rb_mbchronicles_ep2_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""rb_mbchronicles_ep2_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""rb_mbchronicles_ep2_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_rb_christianschiester_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[rb_christianschiester]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>15/04/2012 22:9:45</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_rb_christianschiester_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""rb_christianschiester_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""rb_christianschiester_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_rb_sc_santacatarina_ep4_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[rb_sc_santacatarina_ep4]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>15/04/2012 22:9:45</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_rb_sc_santacatarina_ep4_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""rb_sc_santacatarina_ep4_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""rb_sc_santacatarina_ep4_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_rb_sc_trestles_ep9_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[rb_sc_trestles_ep9]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>15/04/2012 22:9:45</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_rb_sc_trestles_ep9_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""rb_sc_trestles_ep9_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""rb_sc_trestles_ep9_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_rb_sc_peniceportugal_ep11_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[rb_sc_peniceportugal_ep11]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>15/04/2012 22:9:45</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_rb_sc_peniceportugal_ep11_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""rb_sc_peniceportugal_ep11_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""rb_sc_peniceportugal_ep11_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
  <export>
    <media co_guid=""ASSET_rb_mbchronicles_ep1_SS"" action=""insert"" is_active=""true"" Account_Name=""regular"">
      <basic>
        <name>
          <value lang=""eng""><![CDATA[rb_mbchronicles_ep1]]></value>
        </name>
        <description>
          <value lang=""eng""><![CDATA[Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.]]></value>
        </description>
        <media_type>Film</media_type>
        <thumb url=""""/>
        <rules>
          <watch_per_rule>Parent allowed</watch_per_rule>
        </rules>
        <dates>
          <start>15/04/2012 22:9:45</start>
          <catalog_end>12/12/2112 12:12:12</catalog_end>
          <create></create>
          <final_end></final_end>
        </dates>
      </basic>
      <structure>
        <strings>
        </strings>
        <booleans>
        </booleans>
        <doubles>
        </doubles>
        <metas>
        </metas>
      </structure>
      <files>
        <file co_guid=""TRAILER_rb_mbchronicles_ep1_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Trailer"" quality=""HIGH"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""big_buck_bunny_1080p_h264_SS.ism""/>
        <file co_guid=""rb_mbchronicles_ep1_SS"" handling_type=""Clip"" pre_rule="""" break_points="""" type=""PC Main"" quality=""HIGH"" billing_type=""Tvinci"" post_rule="""" cdn_name=""Edgeware"" assetDuration="""" break_rule="""" cdn_code=""rb_mbchronicles_ep1_SS.ism"" PPV_Module=""Subscription Only""/>
      </files>
    </media>
  </export>
</feed>");
    }
}