// Rotator(とHinge)の回転を制御するクラスJointUtil

// xxxxxxxx 既知の不具合 xxxxxxxx
// HingeJoint.angleはジョイントをカプラで傾けているとおかしくなる
// ワールド北向きを0°として|カプラで傾けた角度|*2で半周する謎スケールで
// angleの値に侵食してくる
// その際拾う値はカプラの傾き:ワールド角度の対応でX:方角,Y:ピッチ角,Z:方角
// ただしZ軸のときのみ正負方向の符号の違いもなくなる

using System;
using System.Collections.Generic;
using UnityEngine;

// 名前衝突防止
namespace LibSawUtil{

public class JointUtil
{
	//private static float eps = 0.01f; // 精度(未使用)
	private Vector3 anchor;
	private HingeJoint joint = null;
	private GameObject springCtrlObj;
	private SpringControl sprCtrl;
	// HingeJointを直接触りたいときはこちら
	public HingeJoint Joint{
		get{
			this.CheckJoint();
			return this.joint;
		}
	}
	
	public JointUtil() {}
	// -------- コンストラクタ(座標指定、実行速度を気にしない場合) --------
	// BUILDでのジョイントの座標で指定する
	public JointUtil(Vector3 buildAnchor){
		this.anchor = buildAnchor;
		springCtrlObj = new GameObject();
		springCtrlObj.AddComponent<SpringControl>();
		sprCtrl = springCtrlObj.GetComponent<SpringControl>();
		this.CheckJoint();
		sprCtrl.Init(this.joint);
	}

	// -------- ジョイントの再取得 --------
	private void CheckJoint(){
		// 未取得の場合は取得
		try{
			if(this.joint == null){
				HingeJoint[] hinges = GameObject.FindObjectsOfType(typeof(HingeJoint)) as HingeJoint[];
				foreach (HingeJoint hj in hinges)
				{
					if(hj.anchor == this.anchor){
						this.joint = hj;
						this.sprCtrl.SetJoint(hj);
						break;
					}
				}
			}
		}catch(System.NullReferenceException){
			// ゲームオブジェクトが変更されていた場合ジョイント再取得
			// リスポーン時を想定
			HingeJoint[] hinges = GameObject.FindObjectsOfType(typeof(HingeJoint)) as HingeJoint[];
			foreach (HingeJoint hj in hinges)
			{
				if(hj.anchor == this.anchor){
					this.joint = hj;
					this.sprCtrl.SetJoint(hj);
					break;
				}
			}
		}
	}

	// -------- 指定の力、速度で[強制/制限つき]回転 --------
	// this.Stop()やuseMotor=falseが無い限りここで指定した通りに動き続ける
	// velocityは°/sと思われる
	// freespinをfalseにすると実際の角速度が指定値より速い場合にブレーキが効く
	// trueの場合は加速にのみ使われる
	// ※Unityスクリプトリファレンス(5.4)のfreespinの説明にある「壊れます」は誤り
	// 　原文はbrakeであってbreakではないしマニュアルにもそう書いてある
	// 　翻訳するってレベルじゃねーぞ！
	public void Drive(float force, float velocity, bool freespin){
		this.CheckJoint();
		this.sprCtrl.isEnabled = false;
		this.joint.useSpring = false;
		this.joint.useMotor = true;
		JointMotor motor = this.joint.motor;
		motor.force = force;
		motor.targetVelocity = velocity;
		motor.freeSpin = freespin;
		this.joint.motor = motor;
	}
	// 引数省略版、角速度指定のみ
	// 力とfreeSpinの設定はすでに設定されているものを引き継ぐ
	public void Drive(float velocity){
		this.CheckJoint();
		this.sprCtrl.isEnabled = false;
		this.joint.useSpring = false;
		this.joint.useMotor = true;
		JointMotor motor = this.joint.motor;
		motor.targetVelocity = velocity;
		this.joint.motor = motor;
	}

	// -------- 自由回転・力を加えない --------
	public void Free(){
		this.CheckJoint();
		this.sprCtrl.isEnabled = false;
		this.joint.useSpring = false;
		this.joint.useMotor = false;
	}

	// -------- 角速度取得 --------
	public void Velocity(out float velocity){
		this.CheckJoint(); 
		velocity = this.joint.velocity;
	}

	// -------- 角度取得 --------
	public void Angle(out float angle){
		this.CheckJoint(); 
		angle = Quaternion.Angle(this.Joint.transform.rotation,this.Joint.connectedBody.rotation);
	}

	// -------- マシクラ設定のNeutralに戻す --------
	// マシクラに制御を戻すので、何かアクションを実行中ならその状態になる
	// Rotate(), QRotate()を使用中でもこちらが優先される
	public void Neutral(){
		this.CheckJoint();
		this.sprCtrl.isEnabled = false;
		this.joint.useMotor = false;
		this.joint.useSpring = true;
	}

	// -------- モーターにより指定の角度に到達させる --------
	// ここまでのメソッドと異なり毎フレーム呼ぶ必要がある
	// カプラで傾けたジョイントでは正常に動かない
	// どーーーーしてもSpringを使いたくない場合に使う
	// target : オイラー角(弧度法じゃない方)で角度指定
	// sensitivity : 目標の角度に比例制御で近づける際の反応の鋭さ
	public void RotateM(float force, float target, float sensitivity){
		this.CheckJoint();
		this.sprCtrl.isEnabled = false;
		this.joint.useSpring = false;
		this.joint.useMotor = true;
		JointMotor motor = this.joint.motor;
		motor.force = force;
		float diff = target - this.joint.angle;
		if(Mathf.Abs(diff)>180){
			diff %= 360;
			if(diff > 180){
				diff -=360f;
			}else if(diff < -180){
				diff += 360f;
			}
		}
		motor.targetVelocity = diff*sensitivity;
		motor.freeSpin = false;
		this.joint.motor = motor;
	}

	// -------- スプリングにより指定角度に到達させる --------
	public void Rotate(float damper, float spring, float target){
		this.CheckJoint();
		this.sprCtrl.isEnabled = true;
		JointSpring spr = new JointSpring();
		// 引数をマシクラ内でのスケールと一致(たぶん)させるため定数をかけた上でキャップを設けている
		// 負数入れたときの動作はUnityに準ずる
		spr.damper = (damper>100) ? 300 : (damper * 3);
		spr.spring = (spring>100) ? 3000 :(spring * 30);
		spr.targetPosition = target;
		sprCtrl.jspr_buf = spr;
		sprCtrl.isRequested = true;
	}
	// 引数省略版、角度指定のみ
	// ダンパーおよびスプリングの設定はすでに設定されている値を引き継ぐ
	public void Rotate(float target){
		this.CheckJoint();
		this.sprCtrl.isEnabled = true;
		JointSpring spr = this.joint.spring;
		spr.targetPosition = target;
		sprCtrl.jspr_buf = spr;
		sprCtrl.isRequested = true;
	}

	// -------- Rotateと同じだがキューにより実行が保証される --------
	// 60fps超で動作時にこの関数を毎フレーム呼ぶと
	// UpdateとFixedUpdateの実行間隔の差により消化が追いつかなくなる可能性がある
	// 何かをトリガに1フレームだけ呼ぶというような使い方を想定
	// Rotateと衝突した場合はこちらが優先される
	public void QRotate(float damper, float spring, float target){
		this.CheckJoint();
		this.sprCtrl.isEnabled = true;
		JointSpring spr = new JointSpring();
		spr.damper = (damper>100) ? 300 : (damper * 3);
		spr.spring = (spring>100) ? 3000 :(spring * 30);
		spr.targetPosition = target;
		sprCtrl.RotQue.Enqueue(spr);
	}
	// 引数省略版
	public void QRotate(float target){
		this.CheckJoint();
		this.sprCtrl.isEnabled = true;
		JointSpring spr = this.joint.spring;
		spr.targetPosition = target;
		sprCtrl.RotQue.Enqueue(spr);
	}

	// 色々アレなときに呼ぶ
	public void Reset(){
		this.Neutral();
		this.sprCtrl.isEnabled = false;
		this.sprCtrl.RotQue.Clear();
	}

	~JointUtil(){
		GameObject.Destroy(springCtrlObj);
	}

}// end of JointUtil class

// ======== Springによる角度制御のためFixedUpdateで処理を行うクラス ========
// MonoBehaviourを継承したクラスを作ることで
// UserScriptクラスで禁止されているFixedUpdate()等が使用できる
// あまり人に勧められるものではないが…
public class SpringControl : MonoBehaviour{
	public bool isEnabled;
	private HingeJoint joint;
	public bool isRequested;
	public JointSpring jspr_buf, jspr_mem;
	public Queue<JointSpring> RotQue;

	// 初期化関数
	// ジョイントを参照で設定しておく
	public void Init(HingeJoint hj){
		isEnabled = false;
		this.joint = hj;
		isRequested = false;
		JointSpring spr = new JointSpring();
		spr.damper = 50;
		spr.spring = 100;
		spr.targetPosition = 0;
		jspr_buf = spr;
		jspr_mem = spr;
		RotQue = new Queue<JointSpring>();
	}

	// ジョイントのみ再設定
	// リスポーン時のジョイント再取得で呼ぶ
	public void SetJoint(HingeJoint hj){
		this.joint = hj;
	}
	
	// このタイミングでスプリングを操作するとマシクラ側の値を上書きして反映させられる
	public void FixedUpdate(){
		// このクラスによる制御が有効でなければなにもしない
		if(!this.isEnabled)return;
		// キューに何かあった場合それを優先する
		if(this.RotQue.Count > 0){
			this.jspr_mem = this.RotQue.Dequeue();
		}else if(isRequested){
			this.jspr_mem = this.jspr_buf;
		}
		isRequested = false;
		this.joint.useMotor = false;
		this.joint.useSpring = true;
		this.joint.spring = this.jspr_mem;
	}
}// end of SpringControl Class definition


}// end of namespace LibSawUtil